using Microsoft.EntityFrameworkCore;
using Mo.PersonalWebsite.Infrastructure.Data;
using Mo.PersonalWebsite.Infrastructure.Entities;
using System.Text.RegularExpressions;

namespace Mo.PersonalWebsite.Features.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.Slug = GenerateSlug(category.Name);
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created category: {CategoryName} with ID: {CategoryId}", category.Name, category.Id);
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated category: {CategoryName} with ID: {CategoryId}", category.Name, category.Id);
        return category;
    }

    public async Task DeleteAsync(int id)
    {
        var category = await GetByIdAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted category: {CategoryName} with ID: {CategoryId}", category.Name, category.Id);
        }
    }

    public async Task<Category> GetOrCreateAsync(string name)
    {
        var slug = GenerateSlug(name);
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (existingCategory != null)
        {
            return existingCategory;
        }

        var newCategory = new Category
        {
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(newCategory);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new category: {CategoryName} with ID: {CategoryId}", newCategory.Name, newCategory.Id);
        return newCategory;
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Convert to lowercase
        string slug = input.ToLowerInvariant();

        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }
}
