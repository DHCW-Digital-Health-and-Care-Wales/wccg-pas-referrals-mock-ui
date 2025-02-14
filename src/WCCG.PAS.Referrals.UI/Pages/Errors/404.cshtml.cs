using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WCCG.PAS.Referrals.UI.Pages.Errors;

[ExcludeFromCodeCoverage]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class Error404Model : PageModel
{
    public void OnGet()
    {
    }
}
