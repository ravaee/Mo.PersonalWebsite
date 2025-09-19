using Microsoft.AspNetCore.Mvc;
using Mo.PersonalWebsite.Features.Pages.Services;
using Mo.PersonalWebsite.Infrastructure.Entities;

namespace Mo.PersonalWebsite.Components
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly IPageService _pageService;

        public NavigationViewComponent(IPageService pageService)
        {
            _pageService = pageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var navigationPages = await _pageService.GetNavigationPagesAsync();
            return View(navigationPages);
        }
    }
}
