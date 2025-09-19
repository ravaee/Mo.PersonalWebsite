using Microsoft.Extensions.Options;
using Mo.PersonalWebsite.Features.Auth.AdminLogin.Models;
using BCrypt.Net;

namespace Mo.PersonalWebsite.Features.Auth.AdminLogin.Services;

public interface IAdminAuthService
{
    bool ValidateCredentials(string username, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class AdminAuthService : IAdminAuthService
{
    private readonly AdminUserOptions _adminUserOptions;
    
    public AdminAuthService(IOptions<AdminUserOptions> adminUserOptions)
    {
        _adminUserOptions = adminUserOptions.Value;
    }
    
    public bool ValidateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;
            
        var isUsernameValid = string.Equals(username, _adminUserOptions.Username, StringComparison.OrdinalIgnoreCase);
        var isPasswordValid = VerifyPassword(password, _adminUserOptions.Password);
        
        return isUsernameValid && isPasswordValid;
    }
    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            // If the stored password is not hashed (for development), hash it and compare
            if (!hashedPassword.StartsWith("$2") && hashedPassword.Length < 60)
            {
                return string.Equals(password, hashedPassword, StringComparison.Ordinal);
            }
            
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}
