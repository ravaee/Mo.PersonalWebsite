using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Pages.Articles;

[Authorize]
public class ManageModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ILogger<ManageModel> _logger;

    public ManageModel(IArticleService articleService, ILogger<ManageModel> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    public IEnumerable<Article> Articles { get; set; } = new List<Article>();
    public string SearchTerm { get; set; } = string.Empty;

    public async Task OnGetAsync(string? search)
    {
        SearchTerm = search ?? string.Empty;
        
        var allArticles = await _articleService.GetAllAsync();
        
        if (!string.IsNullOrEmpty(SearchTerm))
        {
            Articles = allArticles.Where(a => 
                a.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.Content.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            Articles = allArticles;
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _articleService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Article deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article with ID {ArticleId}", id);
            TempData["ErrorMessage"] = "Error deleting article. Please try again.";
        }

        return RedirectToPage();
    }
}
