using System.ComponentModel.DataAnnotations;

namespace Mo.PersonalWebsite.Features.Auth.AdminLogin.Models;

public class AdminUserOptions
{
    public const string SectionName = "AdminUser";
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}
