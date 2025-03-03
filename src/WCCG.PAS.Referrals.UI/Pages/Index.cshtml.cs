using Microsoft.AspNetCore.Mvc.RazorPages;
using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class IndexModel : PageModel
{
    private readonly IReferralService _referralService;

    public IndexModel(IReferralService referralService)
    {
        _referralService = referralService;
    }

    public IEnumerable<ReferralDbModel> Referrals { get; set; } = [];

    public async Task OnGet()
    {
        Referrals = await _referralService.GetAllAsync();
    }
}
