using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Pages.Shared;
using WCCG.PAS.Referrals.UI.Services;

namespace WCCG.PAS.Referrals.UI.Pages;

public class IndexModel : ApimSubscriptionKeyModel
{
    private readonly IReferralService _referralService;

    public IndexModel(IReferralService referralService)
    {
        _referralService = referralService;
    }

    public IEnumerable<ReferralDbModel> Referrals { get; set; } = [];

    public async Task OnGet()
    {
        SetApimSubscriptionKey();
        if (string.IsNullOrWhiteSpace(ApimSubscriptionKey))
        {
            HandleEmptyApimKey();
            return;
        }

        try
        {
            Referrals = await _referralService.GetAllAsync(ApimSubscriptionKey);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
