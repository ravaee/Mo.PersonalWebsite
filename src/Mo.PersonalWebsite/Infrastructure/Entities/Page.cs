using System.ComponentModel.DataAnnotations;

namespace Mo.PersonalWebsite.Infrastructure.Entities;

public class Page
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string MetaDescription { get; set; } = string.Empty;
    
    [StringLength(300)]
    public string MetaKeywords { get; set; } = string.Empty;
    
    public bool IsPublished { get; set; } = true;
    
    public bool ShowInNavigation { get; set; } = false;
    
    public int NavigationOrder { get; set; } = 0;
    
    [StringLength(100)]
    public string NavigationText { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string NavigationIcon { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? PublishedAt { get; set; }
}
