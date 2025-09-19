using System.ComponentModel.DataAnnotations;

namespace Mo.PersonalWebsite.Infrastructure.Entities;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(60)]
    public string Slug { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
