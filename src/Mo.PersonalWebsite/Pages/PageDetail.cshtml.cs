using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Pages.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Pages
{
    public class PageDetailModel : PageModel
    {
        private readonly IPageService _pageService;

        public PageDetailModel(IPageService pageService)
        {
            _pageService = pageService;
        }

        public Infrastructure.Entities.Page? PageEntity { get; set; }

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            PageEntity = await _pageService.GetPageBySlugAsync(slug);

            if (PageEntity == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
