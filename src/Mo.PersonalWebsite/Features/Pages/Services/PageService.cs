using Microsoft.EntityFrameworkCore;
using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Features.Pages.Services;

public interface IPageService
{
    Task<IEnumerable<Page>> GetAllPagesAsync();
    Task<IEnumerable<Page>> GetPublishedPagesAsync();
    Task<IEnumerable<Page>> GetNavigationPagesAsync();
    Task<Page?> GetPageByIdAsync(int id);
    Task<Page?> GetPageBySlugAsync(string slug);
    Task<Page> CreatePageAsync(Page page);
    Task<Page> UpdatePageAsync(Page page);
    Task DeletePageAsync(int id);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
}

public class PageService : IPageService
{
    private readonly AppDbContext _context;

    public PageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Page>> GetAllPagesAsync()
    {
        return await _context.Pages
            .OrderBy(p => p.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Page>> GetPublishedPagesAsync()
    {
        return await _context.Pages
            .Where(p => p.IsPublished)
            .OrderBy(p => p.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Page>> GetNavigationPagesAsync()
    {
        return await _context.Pages
            .Where(p => p.IsPublished && p.ShowInNavigation)
            .OrderBy(p => p.NavigationOrder)
            .ThenBy(p => p.NavigationText)
            .ToListAsync();
    }

    public async Task<Page?> GetPageByIdAsync(int id)
    {
        return await _context.Pages.FindAsync(id);
    }

    public async Task<Page?> GetPageBySlugAsync(string slug)
    {
        return await _context.Pages
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
    }

    public async Task<Page> CreatePageAsync(Page page)
    {
        page.CreatedAt = DateTime.UtcNow;
        page.UpdatedAt = DateTime.UtcNow;
        
        if (page.IsPublished && page.PublishedAt == null)
        {
            page.PublishedAt = DateTime.UtcNow;
        }

        _context.Pages.Add(page);
        await _context.SaveChangesAsync();
        return page;
    }

    public async Task<Page> UpdatePageAsync(Page page)
    {
        var existingPage = await _context.Pages.FindAsync(page.Id);
        if (existingPage == null)
        {
            throw new InvalidOperationException("Page not found");
        }

        existingPage.Title = page.Title;
        existingPage.Slug = page.Slug;
        existingPage.Content = page.Content;
        existingPage.MetaDescription = page.MetaDescription;
        existingPage.MetaKeywords = page.MetaKeywords;
        existingPage.ShowInNavigation = page.ShowInNavigation;
        existingPage.NavigationOrder = page.NavigationOrder;
        existingPage.NavigationText = page.NavigationText;
        existingPage.NavigationIcon = page.NavigationIcon;
        existingPage.UpdatedAt = DateTime.UtcNow;

        // Handle publish/unpublish
        if (page.IsPublished && !existingPage.IsPublished)
        {
            existingPage.PublishedAt = DateTime.UtcNow;
        }
        else if (!page.IsPublished && existingPage.IsPublished)
        {
            existingPage.PublishedAt = null;
        }
        
        existingPage.IsPublished = page.IsPublished;

        await _context.SaveChangesAsync();
        return existingPage;
    }

    public async Task DeletePageAsync(int id)
    {
        var page = await _context.Pages.FindAsync(id);
        if (page != null)
        {
            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _context.Pages.Where(p => p.Slug == slug);
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
}
