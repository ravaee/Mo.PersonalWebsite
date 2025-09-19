using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Auth.AdminLogin.DTOs;
using Mo.PersonalWebsite.Features.Auth.AdminLogin.Services;
using System.Security.Claims;

namespace Mo.PersonalWebsite.Pages.Admin;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly ILogger<LoginModel> _logger;
    
    public LoginModel(IAdminAuthService adminAuthService, ILogger<LoginModel> logger)
    {
        _adminAuthService = adminAuthService;
        _logger = logger;
    }
    
    [BindProperty]
    public LoginDto LoginInput { get; set; } = new();
    
    public string? ReturnUrl { get; set; }
    
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Admin/Dashboard");
        }
        
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        ReturnUrl = returnUrl;
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        try
        {
            var isValid = _adminAuthService.ValidateCredentials(LoginInput.Username, LoginInput.Password);
            
            if (isValid)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, LoginInput.Username),
                    new(ClaimTypes.Role, "Admin"),
                    new("IsAdmin", "true")
                };
                
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = LoginInput.RememberMe,
                    ExpiresUtc = LoginInput.RememberMe 
                        ? DateTimeOffset.UtcNow.AddDays(30) 
                        : DateTimeOffset.UtcNow.AddHours(8)
                };
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProps);
                
                _logger.LogInformation("Admin user {Username} logged in successfully", LoginInput.Username);
                
                return LocalRedirect(returnUrl ?? "/Admin/Dashboard");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                _logger.LogWarning("Failed login attempt for username: {Username}", LoginInput.Username);
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for username: {Username}", LoginInput.Username);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Admin user logged out");
        return RedirectToPage("/Index");
    }
}
