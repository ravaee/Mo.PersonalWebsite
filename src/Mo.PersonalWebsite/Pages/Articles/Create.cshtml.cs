using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mo.PersonalWebsite.Features.Articles.Services;
using Mo.PersonalWebsite.Features.Categories.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Mo.PersonalWebsite.Pages.Articles;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IArticleService articleService, ICategoryService categoryService, ILogger<CreateModel> logger)
    {
        _articleService = articleService;
        _categoryService = categoryService;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    public List<SelectListItem> Categories { get; set; } = new();

    public class InputModel
    {
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

        [Display(Name = "Publish immediately")]
        public bool IsPublished { get; set; }
        
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        
        [Display(Name = "New Category")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string? NewCategoryName { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        try
        {
            var slug = !string.IsNullOrEmpty(Input.Slug) 
                ? GenerateSlug(Input.Slug) 
                : GenerateSlug(Input.Title);

            // Handle category assignment or creation
            int? categoryId = null;
            if (!string.IsNullOrWhiteSpace(Input.NewCategoryName))
            {
                var newCategory = await _categoryService.GetOrCreateAsync(Input.NewCategoryName);
                categoryId = newCategory.Id;
            }
            else if (Input.CategoryId.HasValue && Input.CategoryId.Value > 0)
            {
                categoryId = Input.CategoryId;
            }
            else
            {
                // If no category is selected, assign to "General"
                var generalCategory = await _categoryService.GetOrCreateAsync("General");
                categoryId = generalCategory.Id;
            }

            var article = new Article
            {
                Title = Input.Title,
                Slug = slug,
                Content = Input.Content,
                MetaDescription = Input.MetaDescription,
                MetaKeywords = Input.MetaKeywords,
                IsPublished = Input.IsPublished,
                PublishedAt = Input.IsPublished ? DateTime.UtcNow : null,
                CategoryId = categoryId
            };

            await _articleService.CreateAsync(article);

            TempData["SuccessMessage"] = Input.IsPublished 
                ? "Article published successfully!" 
                : "Article saved as draft successfully!";

            return RedirectToPage("/Articles/Manage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            ModelState.AddModelError(string.Empty, "An error occurred while saving the article. Please try again.");
            await LoadCategoriesAsync();
            return Page();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        Categories = new List<SelectListItem>
        {
            new() { Value = "", Text = "-- Select Category --" }
        };
        Categories.AddRange(categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }));
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
