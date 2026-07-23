using Contas_Db.Model.Interface;

namespace Contas_Db.Repository.Interface;

public interface IRepository<T> where T : class, ISoftDelete
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
}
