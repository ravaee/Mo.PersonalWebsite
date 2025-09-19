using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Pages.Articles;

public class DetailModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ILogger<DetailModel> _logger;
    
    public DetailModel(IArticleService articleService, ILogger<DetailModel> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }
    
    public Article? Article { get; set; }
    public IEnumerable<Article> RelatedArticles { get; set; } = new List<Article>();
    
    public async Task<IActionResult> OnGetAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }
        
        try
        {
            Article = await _articleService.GetBySlugAsync(slug);
            
            if (Article == null || !Article.IsPublished)
            {
                return NotFound();
            }
            
            // Set meta data for SEO
            ViewData["Title"] = Article.Title;
            ViewData["MetaDescription"] = Article.MetaDescription ?? Article.Title;
            ViewData["MetaKeywords"] = string.Join(", ", Article.Tags.Select(t => t.Name));
            
            // Get related articles (same tags, excluding current article)
            var allArticles = await _articleService.GetAllPublishedAsync();
            RelatedArticles = allArticles
                .Where(a => a.Id != Article.Id && a.Tags.Any(t => Article.Tags.Any(at => at.Id == t.Id)))
                .Take(3);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading article with slug: {Slug}", slug);
            return NotFound();
        }
    }
}
