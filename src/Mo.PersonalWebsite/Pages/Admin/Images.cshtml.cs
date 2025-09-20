using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Mo.PersonalWebsite.Features.Images.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ImagesModel : PageModel
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImagesModel> _logger;

    public ImagesModel(IImageService imageService, ILogger<ImagesModel> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    public IEnumerable<ImageBlob> Images { get; set; } = new List<ImageBlob>();

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    public string? AltText { get; set; }

    [BindProperty]
    public string? Caption { get; set; }

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task OnGetAsync()
    {
        await LoadImages();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (UploadFile == null || UploadFile.Length == 0)
        {
            Message = "Please select a file to upload.";
            IsSuccess = false;
            await LoadImages();
            return Page();
        }

        try
        {
            var uploadedImage = await _imageService.SaveAsync(UploadFile, AltText, Caption);
            Message = $"Image '{uploadedImage.OriginalFileName}' uploaded successfully!";
            IsSuccess = true;
            
            // Clear form fields
            UploadFile = null;
            AltText = null;
            Caption = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            Message = "Error uploading image. Please try again.";
            IsSuccess = false;
        }

        await LoadImages();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _imageService.DeleteAsync(id);
            Message = "Image deleted successfully!";
            IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with ID {ImageId}", id);
            Message = "Error deleting image. Please try again.";
            IsSuccess = false;
        }

        await LoadImages();
        return Page();
    }

    private async Task LoadImages()
    {
        Images = await _imageService.GetAllAsync();
    }
}