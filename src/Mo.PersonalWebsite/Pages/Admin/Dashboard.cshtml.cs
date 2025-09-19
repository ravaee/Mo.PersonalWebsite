using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mo.PersonalWebsite.Pages.Admin;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    public void OnGet()
    {
    }
}
