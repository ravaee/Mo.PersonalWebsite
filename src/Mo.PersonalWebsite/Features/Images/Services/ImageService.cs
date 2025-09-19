using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mo.PersonalWebsite.Features.Images.Services;

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
            throw new ArgumentException("File is required", nameof(file));
            
        var fileName = await GenerateUniqueFileName(file.FileName);
        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        
        // Ensure uploads directory exists
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
        
        var filePath = Path.Combine(uploadsPath, fileName);
        
        // Save file to disk
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        
        // Get image dimensions if it's an image
        int? width = null, height = null;
        try
        {
            using (var image = SixLabors.ImageSharp.Image.Load(filePath))
            {
                width = image.Width;
                height = image.Height;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read image dimensions for file: {FileName}", fileName);
        }
        
        var imageBlob = new ImageBlob
        {
            FileName = fileName,
            OriginalFileName = file.FileName,
            FilePath = $"/uploads/{fileName}",
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            AltText = altText,
            Caption = caption,
            Width = width,
            Height = height,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.ImageBlobs.Add(imageBlob);
        await _context.SaveChangesAsync();
        
        return imageBlob;
    }
    
    public async Task DeleteAsync(int id)
    {
        var imageBlob = await _context.ImageBlobs.FindAsync(id);
        if (imageBlob != null)
        {
            // Delete file from disk
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", imageBlob.FileName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
                }
            }
            
            _context.ImageBlobs.Remove(imageBlob);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<string> GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var baseFileName = Path.GetFileNameWithoutExtension(originalFileName);
        
        // Create a unique filename using timestamp and GUID
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"{baseFileName}_{timestamp}_{uniqueId}{extension}";
        
        // Ensure the filename doesn't already exist in the database
        var existingFile = await _context.ImageBlobs
            .FirstOrDefaultAsync(i => i.FileName == fileName);
            
        if (existingFile != null)
        {
            // If somehow it exists, add more uniqueness
            fileName = $"{baseFileName}_{timestamp}_{Guid.NewGuid():N}{extension}";
        }
        
        return fileName;
    }
}
