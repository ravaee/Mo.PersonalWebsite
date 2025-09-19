using System.ComponentModel.DataAnnotations;

namespace Mo.PersonalWebsite.Features.Auth.AdminLogin.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
    
    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RedirectUrl { get; set; }
}
