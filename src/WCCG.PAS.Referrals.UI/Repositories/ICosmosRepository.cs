namespace WCCG.PAS.Referrals.UI.Repositories;

public interface ICosmosRepository<T>
{
    Task<bool> UpsertAsync(string apimKey, T item);
    Task<IEnumerable<T>> GetAllAsync(string apimKey);
    Task<T> GetByIdAsync(string apimKey, string id);
}
