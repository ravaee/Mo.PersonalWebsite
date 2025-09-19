using System.ComponentModel.DataAnnotations;

namespace Mo.PersonalWebsite.Infrastructure.Entities;

public class ImageBlob
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    [MaxLength(200)]
    public string? AltText { get; set; }
    
    [MaxLength(300)]
    public string? Caption { get; set; }
    
    public int? Width { get; set; }
    public int? Height { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
