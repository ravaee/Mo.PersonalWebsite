using System.ComponentModel.DataAnnotations;

namespace Mo.PersonalWebsite.Infrastructure.Entities;

public class Article
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(250)]
    public string Slug { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(300)]
    public string? MetaDescription { get; set; }
    
    [MaxLength(500)]
    public string? MetaKeywords { get; set; }
    
    public bool IsPublished { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Foreign keys
    public int? CategoryId { get; set; }
    
    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    public ICollection<Tag> Tags => ArticleTags.Select(at => at.Tag).ToList();
}

// Junction table for many-to-many relationship
public class ArticleTag
{
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
