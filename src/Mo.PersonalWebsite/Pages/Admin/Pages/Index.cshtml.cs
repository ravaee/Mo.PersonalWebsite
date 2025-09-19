using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mo.PersonalWebsite.Features.Pages.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Pages.Admin.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPageService _pageService;

        public IndexModel(IPageService pageService)
        {
            _pageService = pageService;
        }

        public List<Infrastructure.Entities.Page> Pages { get; set; } = new();

        public async Task OnGetAsync()
        {
            var pages = await _pageService.GetAllPagesAsync();
            Pages = pages.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _pageService.DeletePageAsync(id);
            return RedirectToPage();
        }
    }
}
