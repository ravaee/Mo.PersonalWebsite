using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Features.Categories.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;
using Mo.PersonalWebsite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Mo.PersonalWebsite.Pages.Articles;

public class ArticlesIndexModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<ArticlesIndexModel> _logger;
    private readonly AppDbContext _context;
    
    public ArticlesIndexModel(IArticleService articleService, ICategoryService categoryService, ILogger<ArticlesIndexModel> logger, AppDbContext context)
    {
        _articleService = articleService;
        _categoryService = categoryService;
        _logger = logger;
        _context = context;
    }

    public IEnumerable<Article> Articles { get; set; } = new List<Article>();
    public string? SelectedCategory { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalArticles { get; set; }
    public List<CategoryInfo> AllCategories { get; set; } = new();
    
    // Pagination properties
    public int PageSize { get; set; } = 12;
    public int TotalPages { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public async Task OnGetAsync()
    {
        // Explicitly get parameters from query string
        var pageParam = Request.Query["page"].FirstOrDefault();
        var categoryParam = Request.Query["category"].FirstOrDefault();
        
        SelectedCategory = categoryParam;
        CurrentPage = Math.Max(1, int.TryParse(pageParam, out var page) ? page : 1);
        
        try
        {
            // Get paginated articles
            var (articles, totalCount) = await _articleService.GetPublishedWithPaginationAsync(CurrentPage, PageSize, SelectedCategory);
            Articles = articles;
            TotalArticles = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            
            // Build category aggregation efficiently using database queries
            var allCategories = await _categoryService.GetAllAsync();
            AllCategories = new List<CategoryInfo>();
            
            foreach (var cat in allCategories)
            {
                // Get just the count for each category efficiently
                var categoryQuery = _context.Articles
                    .Where(a => a.IsPublished && a.Category != null && a.Category.Slug == cat.Slug);
                var categoryCount = await categoryQuery.CountAsync();
                
                if (categoryCount > 0)
                {
                    AllCategories.Add(new CategoryInfo 
                    { 
                        Name = cat.Name, 
                        Slug = cat.Slug, 
                        Count = categoryCount 
                    });
                }
            }
            
            AllCategories = AllCategories.OrderByDescending(c => c.Count).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading articles");
            Articles = new List<Article>();
            AllCategories = new();
        }
    }

    public class CategoryInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
