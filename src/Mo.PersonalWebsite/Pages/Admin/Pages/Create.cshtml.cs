using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Pages.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Mo.PersonalWebsite.Pages.Admin.Pages
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IPageService _pageService;

        public CreateModel(IPageService pageService)
        {
            _pageService = pageService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
            [Display(Name = "Page Title")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Slug is required")]
            [StringLength(200, ErrorMessage = "Slug cannot exceed 200 characters")]
            [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
            [Display(Name = "URL Slug")]
            public string Slug { get; set; } = string.Empty;

            [Required(ErrorMessage = "Content is required")]
            [Display(Name = "Page Content")]
            public string Content { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Meta description cannot exceed 500 characters")]
            [Display(Name = "Meta Description")]
            public string MetaDescription { get; set; } = string.Empty;

            [StringLength(300, ErrorMessage = "Meta keywords cannot exceed 300 characters")]
            [Display(Name = "Meta Keywords")]
            public string MetaKeywords { get; set; } = string.Empty;

            [Display(Name = "Published")]
            public bool IsPublished { get; set; } = true;

            [Display(Name = "Show in Navigation")]
            public bool ShowInNavigation { get; set; } = false;

            [Display(Name = "Navigation Order")]
            public int NavigationOrder { get; set; } = 0;

            [StringLength(100, ErrorMessage = "Navigation text cannot exceed 100 characters")]
            [Display(Name = "Navigation Text")]
            public string NavigationText { get; set; } = string.Empty;

            [StringLength(50, ErrorMessage = "Navigation icon cannot exceed 50 characters")]
            [Display(Name = "Navigation Icon (Bootstrap Icon class)")]
            public string NavigationIcon { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if slug already exists
            if (await _pageService.SlugExistsAsync(Input.Slug))
            {
                ModelState.AddModelError("Input.Slug", "A page with this slug already exists");
                return Page();
            }

            var page = new Infrastructure.Entities.Page
            {
                Title = Input.Title,
                Slug = Input.Slug,
                Content = Input.Content,
                MetaDescription = Input.MetaDescription,
                MetaKeywords = Input.MetaKeywords,
                IsPublished = Input.IsPublished,
                ShowInNavigation = Input.ShowInNavigation,
                NavigationOrder = Input.NavigationOrder,
                NavigationText = Input.NavigationText,
                NavigationIcon = Input.NavigationIcon
            };

            await _pageService.CreatePageAsync(page);

            TempData["SuccessMessage"] = "Page created successfully!";
            return RedirectToPage("Index");
        }

        public IActionResult OnPostGenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new JsonResult(new { slug = "" });
            }

            var slug = GenerateSlug(title);
            return new JsonResult(new { slug });
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Convert to lowercase and replace spaces with hyphens
            var slug = title.ToLowerInvariant().Trim();
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}
