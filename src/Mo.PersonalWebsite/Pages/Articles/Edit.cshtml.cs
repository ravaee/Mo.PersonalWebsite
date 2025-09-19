using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Mo.PersonalWebsite.Pages.Articles;

[Authorize]
public class EditModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IArticleService articleService, ILogger<EditModel> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Article? Article { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Slug cannot exceed 250 characters.")]
        public string? Slug { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Meta description cannot exceed 300 characters.")]
        [Display(Name = "Meta Description")]
        public string? MetaDescription { get; set; }

        [StringLength(500, ErrorMessage = "Meta keywords cannot exceed 500 characters.")]
        [Display(Name = "Meta Keywords")]
        public string? MetaKeywords { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Article = await _articleService.GetByIdAsync(id);
        
        if (Article == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = Article.Id,
            Title = Article.Title,
            Slug = Article.Slug,
            Content = Article.Content,
            MetaDescription = Article.MetaDescription,
            MetaKeywords = Article.MetaKeywords,
            IsPublished = Article.IsPublished
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Article = await _articleService.GetByIdAsync(Input.Id);
            return Page();
        }

        try
        {
            var existingArticle = await _articleService.GetByIdAsync(Input.Id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            var slug = !string.IsNullOrEmpty(Input.Slug) 
                ? GenerateSlug(Input.Slug) 
                : GenerateSlug(Input.Title);

            // Update the article properties
            existingArticle.Title = Input.Title;
            existingArticle.Slug = slug;
            existingArticle.Content = Input.Content;
            existingArticle.MetaDescription = Input.MetaDescription;
            existingArticle.MetaKeywords = Input.MetaKeywords;
            
            // Handle publishing status
            if (Input.IsPublished && !existingArticle.IsPublished)
            {
                // Publishing for the first time
                existingArticle.IsPublished = true;
                existingArticle.PublishedAt = DateTime.UtcNow;
            }
            else if (!Input.IsPublished && existingArticle.IsPublished)
            {
                // Unpublishing
                existingArticle.IsPublished = false;
                // Keep PublishedAt for history
            }
            else
            {
                existingArticle.IsPublished = Input.IsPublished;
            }

            await _articleService.UpdateAsync(existingArticle);

            TempData["SuccessMessage"] = "Article updated successfully!";
            return RedirectToPage("/Articles/Manage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article with ID {ArticleId}", Input.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the article. Please try again.");
            Article = await _articleService.GetByIdAsync(Input.Id);
            return Page();
        }
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
