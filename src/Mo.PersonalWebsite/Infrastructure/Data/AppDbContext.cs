using Microsoft.EntityFrameworkCore;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Article> Articles { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ImageBlob> ImageBlobs { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<Page> Pages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Article entity
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Articles)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
        
        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
        
        // Configure ImageBlob entity
        modelBuilder.Entity<ImageBlob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FileName).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
        
        // Configure ArticleTag junction table
        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.TagId });
            
            entity.HasOne(e => e.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure Page entity
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
