using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mo.PersonalWebsite.Features.Articles.Services;

public interface IArticleService
{
    Task<IEnumerable<Article>> GetAllPublishedAsync();
    Task<IEnumerable<Article>> GetLatestPublishedAsync(int count = 10);
    Task<(IEnumerable<Article> Articles, int TotalCount)> GetPublishedWithPaginationAsync(int page = 1, int pageSize = 12, string? category = null);
    Task<Article?> GetBySlugAsync(string slug);
    Task<Article?> GetByIdAsync(int id);
    Task<Article> CreateAsync(Article article);
    Task<Article> UpdateAsync(Article article);
    Task DeleteAsync(int id);
    Task<IEnumerable<Article>> GetAllAsync();
}

public class ArticleService : IArticleService
{
    private readonly AppDbContext _context;
    
    public ArticleService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Article>> GetAllPublishedAsync()
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetLatestPublishedAsync(int count = 10)
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Article> Articles, int TotalCount)> GetPublishedWithPaginationAsync(int page = 1, int pageSize = 12, string? category = null)
    {
        var query = _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .Where(a => a.IsPublished);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(a => a.Category != null && a.Category.Slug == category);
        }

        var totalCount = await query.CountAsync();
        
        var articles = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (articles, totalCount);
    }
    
    public async Task<Article?> GetBySlugAsync(string slug)
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Slug == slug);
    }
    
    public async Task<Article?> GetByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Article> CreateAsync(Article article)
    {
        article.CreatedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;
        
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }
    
    public async Task<Article> UpdateAsync(Article article)
    {
        article.UpdatedAt = DateTime.UtcNow;
        
        _context.Articles.Update(article);
        await _context.SaveChangesAsync();
        return article;
    }
    
    public async Task DeleteAsync(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article != null)
        {
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
