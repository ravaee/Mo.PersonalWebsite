using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Pages;

public class IndexModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ILogger<IndexModel> _logger;
    
    public IndexModel(IArticleService articleService, ILogger<IndexModel> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }
    
    public IEnumerable<Article> RecentArticles { get; set; } = new List<Article>();
    
    public async Task OnGetAsync()
    {
        try
        {
            RecentArticles = await _articleService.GetLatestPublishedAsync(10);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent articles for home page");
            RecentArticles = new List<Article>();
        }
    }
}
