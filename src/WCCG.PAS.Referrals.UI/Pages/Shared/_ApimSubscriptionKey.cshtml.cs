using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WCCG.PAS.Referrals.UI.Pages.Shared;

public class ApimSubscriptionKeyModel : PageModel
{
    private const string ApimKey = "ApimKey";

    [BindProperty]
    public string? ApimSubscriptionKey { get; set; }

    public string? ErrorMessage { get; set; }

    public IActionResult OnPostApimKey(string apimSubscriptionKey)
    {
        ApimSubscriptionKey = apimSubscriptionKey;

        if (string.IsNullOrWhiteSpace(ApimSubscriptionKey))
        {
            HandleEmptyApimKey();
            return Redirect(Request.Headers.Referer.ToString());
        }

        HttpContext.Response.Cookies.Append(ApimKey, ApimSubscriptionKey, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMonths(1)
        });

        return Redirect(Request.Headers.Referer.ToString());
    }

    protected void HandleEmptyApimKey()
    {
        HttpContext.Response.Cookies.Delete(ApimKey);
        ErrorMessage = "APIM key value required";
    }

    protected void SetApimSubscriptionKey()
    {
        ApimSubscriptionKey = HttpContext.Request.Cookies[ApimKey];
    }
}
