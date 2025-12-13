using System.Linq.Expressions;

namespace Infrastructure.Repositories.BaseRepository;

public interface IBaseRepository<T>
{
    IQueryable<T> GetAll();
    Task<T> GetAsync(Expression<Func<T, bool>> expression);
    Task<T> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task<T> UpdateAsync(T entity);
    Task<T> UpdateAsync(int id, T entity);
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);
    Task<T> DeleteAsync(int id);
    Task<T> DeleteAsync(Expression<Func<T, bool>> expression);
    Task<IEnumerable<T>> DeleteRangeAsync(IEnumerable<T> entities);

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync(Exception ex);
}