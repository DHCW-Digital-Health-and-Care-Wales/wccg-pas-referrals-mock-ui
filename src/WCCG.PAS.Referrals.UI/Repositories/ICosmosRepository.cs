namespace WCCG.PAS.Referrals.UI.Repositories;

public interface ICosmosRepository<T>
{
    Task<bool> UpsertAsync(T item);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(string id);
}
