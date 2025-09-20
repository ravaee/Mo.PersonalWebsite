using Microsoft.AspNetCore.Mvc;
using Mo.PersonalWebsite.Features.Images.Services;

namespace Mo.PersonalWebsite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetImages()
    {
        try
        {
            var images = await _imageService.GetAllAsync();
            var imageList = images.Select(img => new
            {
                id = img.Id,
                fileName = img.FileName,
                originalFileName = img.OriginalFileName,
                filePath = img.FilePath,
                altText = img.AltText ?? img.OriginalFileName,
                width = img.Width,
                height = img.Height
            });

            return Ok(imageList);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to load images" });
        }
    }
}