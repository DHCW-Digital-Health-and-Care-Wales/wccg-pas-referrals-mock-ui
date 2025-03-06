using WCCG.PAS.Referrals.UI.DbModels;
using WCCG.PAS.Referrals.UI.Repositories;

namespace WCCG.PAS.Referrals.UI.Services;

public class ReferralService : IReferralService
{
    private readonly ICosmosRepository<ReferralDbModel> _repository;

    public ReferralService(ICosmosRepository<ReferralDbModel> repository)
    {
        _repository = repository;
    }

    public async Task<bool> UpsertAsync(string apimKey, ReferralDbModel item)
    {
        return await _repository.UpsertAsync(apimKey, item);
    }

    public async Task<IEnumerable<ReferralDbModel>> GetAllAsync(string apimKey)
    {
        return await _repository.GetAllAsync(apimKey);
    }

    public async Task<ReferralDbModel> GetByIdAsync(string apimKey, string id)
    {
        return await _repository.GetByIdAsync(apimKey, id);
    }
}
