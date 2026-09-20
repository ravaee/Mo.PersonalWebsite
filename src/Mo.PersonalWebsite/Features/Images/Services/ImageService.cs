using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Infrastructure.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Mo.PersonalWebsite.Features.Images.Services;

/// <summary>
/// Thrown when an upload is rejected for a reason worth showing to the user.
/// </summary>
public class ImageValidationException : Exception
{
    public ImageValidationException(string message) : base(message) { }
}

public interface IImageService
{
    Task<IEnumerable<ImageBlob>> GetAllAsync();
    Task<ImageBlob?> GetByIdAsync(int id);
    Task<ImageBlob?> GetByFileNameAsync(string fileName);
    Task<ImageBlob> SaveAsync(IFormFile file, string? altText = null, string? caption = null);
    Task DeleteAsync(int id);
    Task<string> GenerateUniqueFileName(string originalFileName);
}

public class ImageService : IImageService
{
    // Only formats every browser can display are stored, and they are stored
    // byte-for-byte: re-encoding a GIF here would cost it its animation.
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const long MaxPixels = 50_000_000;
    private const int MaxAltTextLength = 200;
    private const int MaxCaptionLength = 300;

    private static readonly string[] AcceptedExtensions = [".jpg", ".jpeg", ".png", ".gif"];

    private const string HeicGuidance =
        "HEIC photos cannot be shown by web browsers. On iPhone set " +
        "Settings > Camera > Formats to \"Most Compatible\", or export the photo " +
        "as JPG from the Photos app, then upload it.";

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageService> _logger;

    public ImageService(AppDbContext context, IWebHostEnvironment environment, ILogger<ImageService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IEnumerable<ImageBlob>> GetAllAsync()
    {
        return await _context.ImageBlobs
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<ImageBlob?> GetByIdAsync(int id)
    {
        return await _context.ImageBlobs.FindAsync(id);
    }

    public async Task<ImageBlob?> GetByFileNameAsync(string fileName)
    {
        return await _context.ImageBlobs
            .FirstOrDefaultAsync(i => i.FileName == fileName);
    }

    public async Task<ImageBlob> SaveAsync(IFormFile file, string? altText = null, string? caption = null)
    {
        if (file == null || file.Length == 0)
            throw new ImageValidationException("Please choose a file to upload.");

        if (file.Length > MaxFileSizeBytes)
            throw new ImageValidationException(
                $"This file is {Describe(file.Length)}. The maximum upload size is {Describe(MaxFileSizeBytes)}.");

        if (altText is { Length: > MaxAltTextLength })
            throw new ImageValidationException($"Alt text must be {MaxAltTextLength} characters or fewer.");

        if (caption is { Length: > MaxCaptionLength })
            throw new ImageValidationException($"Caption must be {MaxCaptionLength} characters or fewer.");

        // Path.GetFileName strips any directory the browser may have sent, so a
        // name like "../../appsettings.json" cannot escape the uploads folder.
        var originalFileName = Path.GetFileName(file.FileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ImageValidationException("The uploaded file has no name.");

        var uploadedExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (uploadedExtension is ".heic" or ".heif")
            throw new ImageValidationException(HeicGuidance);

        if (!AcceptedExtensions.Contains(uploadedExtension))
            throw new ImageValidationException("Only JPG, PNG and GIF images can be uploaded.");

        // Buffer the upload so its real content can be inspected before anything is written to disk
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);
        buffer.Position = 0;

        var processed = ProcessUpload(buffer, originalFileName);

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = await GenerateUniqueFileName(
            Path.ChangeExtension(originalFileName, processed.Extension.TrimStart('.')));

        var filePath = Path.GetFullPath(Path.Combine(uploadsPath, fileName));
        var uploadsRoot = Path.GetFullPath(uploadsPath) + Path.DirectorySeparatorChar;
        if (!filePath.StartsWith(uploadsRoot, StringComparison.Ordinal))
            throw new ImageValidationException("The file name is not valid.");

        await File.WriteAllBytesAsync(filePath, processed.Content);

        var imageBlob = new ImageBlob
        {
            FileName = fileName,
            OriginalFileName = Truncate(originalFileName, 255),
            FilePath = $"/uploads/{fileName}",
            // Taken from the bytes themselves, never from the client-supplied content type
            ContentType = processed.ContentType,
            FileSizeBytes = processed.Content.Length,
            AltText = altText,
            Caption = caption,
            Width = processed.Width,
            Height = processed.Height,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ImageBlobs.Add(imageBlob);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Otherwise the file lingers in the uploads folder with nothing pointing at it
            TryDeleteFile(filePath);
            throw;
        }

        return imageBlob;
    }

    /// <summary>
    /// Validates that the uploaded bytes really are a JPEG, PNG or GIF, and returns
    /// them unchanged along with the extension and content type the file itself claims.
    /// </summary>
    private ProcessedImage ProcessUpload(MemoryStream buffer, string originalFileName)
    {
        ImageInfo? info = null;
        try
        {
            info = Image.Identify(buffer);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ImageSharp could not identify {FileName}", originalFileName);
        }
        finally
        {
            buffer.Position = 0;
        }

        if (info?.Metadata.DecodedImageFormat is null)
        {
            // A HEIC renamed to .jpg reaches this point, so name the real problem
            throw new ImageValidationException(
                LooksLikeHeic(buffer)
                    ? HeicGuidance
                    : "This file could not be read as an image. Upload a JPG, PNG or GIF image.");
        }

        GuardPixelCount(info.Width, info.Height);

        var (extension, contentType) = info.Metadata.DecodedImageFormat switch
        {
            JpegFormat => (".jpg", "image/jpeg"),
            PngFormat => (".png", "image/png"),
            GifFormat => (".gif", "image/gif"),
            _ => (string.Empty, string.Empty)
        };

        if (extension.Length == 0)
        {
            throw new ImageValidationException(
                $"{info.Metadata.DecodedImageFormat.Name} images are not supported. " +
                "Upload a JPG, PNG or GIF image.");
        }

        return new ProcessedImage(buffer.ToArray(), extension, contentType, info.Width, info.Height);
    }

    /// <summary>
    /// HEIC/HEIF is an ISO base media file: "ftyp" at offset 4, then a brand
    /// such as "heic", "heix", "mif1" or "msf1".
    /// </summary>
    private static bool LooksLikeHeic(MemoryStream buffer)
    {
        try
        {
            if (buffer.Length < 12)
                return false;

            buffer.Position = 0;
            var header = new byte[12];
            if (buffer.Read(header, 0, header.Length) < header.Length)
                return false;

            if (Encoding.ASCII.GetString(header, 4, 4) != "ftyp")
                return false;

            var brand = Encoding.ASCII.GetString(header, 8, 4).ToLowerInvariant();
            return brand.StartsWith("hei", StringComparison.Ordinal)
                || brand.StartsWith("hev", StringComparison.Ordinal)
                || brand is "mif1" or "msf1";
        }
        finally
        {
            buffer.Position = 0;
        }
    }

    private static void GuardPixelCount(int width, int height)
    {
        if ((long)width * height > MaxPixels)
        {
            throw new ImageValidationException(
                $"This image is {width}x{height} pixels, which is too large to process.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var imageBlob = await _context.ImageBlobs.FindAsync(id);
        if (imageBlob != null)
        {
            TryDeleteFile(Path.Combine(
                _environment.WebRootPath, "uploads", Path.GetFileName(imageBlob.FileName)));

            _context.ImageBlobs.Remove(imageBlob);
            await _context.SaveChangesAsync();
        }
    }

    private void TryDeleteFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
        }
    }

    public async Task<string> GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        // The stored name ends up in a URL and in generated HTML, so reduce it to
        // characters that need no escaping in either place.
        var baseFileName = Slugify(Path.GetFileNameWithoutExtension(originalFileName));

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{baseFileName}_{timestamp}_{uniqueId}{extension}";

        var existingFile = await _context.ImageBlobs
            .FirstOrDefaultAsync(i => i.FileName == fileName);

        if (existingFile != null)
        {
            fileName = $"{baseFileName}_{timestamp}_{Guid.NewGuid():N}{extension}";
        }

        return fileName;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            if (character < 128 && char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character));
            else
                builder.Append('-');
        }

        var slug = Regex.Replace(builder.ToString(), "-{2,}", "-").Trim('-');
        if (slug.Length > 60)
            slug = slug[..60].Trim('-');

        return slug.Length == 0 ? "image" : slug;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static string Describe(long bytes) =>
        string.Create(CultureInfo.InvariantCulture, $"{bytes / 1024.0 / 1024.0:F1} MB");

    private sealed record ProcessedImage(
        byte[] Content,
        string Extension,
        string ContentType,
        int Width,
        int Height);
}
