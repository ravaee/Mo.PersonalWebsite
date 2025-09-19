using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Mo.PersonalWebsite.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorPageModel : PageModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string ErrorMessage { get; set; } = string.Empty;
    public new int StatusCode { get; set; }

    private readonly ILogger<ErrorPageModel> _logger;

    public ErrorPageModel(ILogger<ErrorPageModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int statusCode = 500)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        StatusCode = statusCode;
        
        ErrorMessage = StatusCode switch
        {
            404 => "The page you're looking for doesn't exist.",
            403 => "You don't have permission to access this resource.",
            500 => "An internal server error occurred.",
            _ => "An unexpected error occurred."
        };
        
        _logger.LogError("Error page accessed with status code: {StatusCode}, RequestId: {RequestId}", StatusCode, RequestId);
    }
}
