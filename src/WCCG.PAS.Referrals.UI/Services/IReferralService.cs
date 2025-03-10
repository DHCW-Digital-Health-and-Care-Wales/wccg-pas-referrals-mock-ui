using WCCG.PAS.Referrals.UI.DbModels;

namespace WCCG.PAS.Referrals.UI.Services;

public interface IReferralService
{
    Task<bool> UpsertAsync(string apimKey, ReferralDbModel item);
    Task<IEnumerable<ReferralDbModel>> GetAllAsync(string apimKey);
    Task<ReferralDbModel> GetByIdAsync(string apimKey, string id);
}
