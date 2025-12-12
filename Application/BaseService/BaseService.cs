using System.Linq.Expressions;
using Domain.Entities.Base;
using Infrastructure.Repositories.BaseRepository;

namespace Application.BaseService;

public class BaseService<T>(IBaseRepository<T> baseRepository) : IBaseService<T> where T : BaseEntity
{
    public IQueryable<T> GetAll() => baseRepository.GetAll();
    public async Task<T> GetAsync(Expression<Func<T, bool>> expression) => await baseRepository.GetAsync(expression);
    public async Task<T> GetByIdAsync(int id) => await baseRepository.GetByIdAsync(id);

    public async Task<T> AddAsync(T entity) => await baseRepository.AddAsync(entity);
    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entity) => await baseRepository.AddRangeAsync(entity);

    public async Task<T> UpdateAsync(int id, T entity) => await baseRepository.UpdateAsync(id, entity);
    public async Task<T> UpdateAsync(T entity) => await baseRepository.UpdateAsync(entity);
    public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities) => await baseRepository.UpdateRangeAsync(entities);

    public async Task<T> DeleteAsync(int id) => await baseRepository.DeleteAsync(id);
    public async Task<T> DeleteAsync(Expression<Func<T, bool>> expression) => await baseRepository.DeleteAsync(expression);
    public async Task DeleteRangeAsync(IEnumerable<T> entities) => await baseRepository.DeleteRangeAsync(entities);


    public async Task BeginTransactionAsync() => await baseRepository.BeginTransactionAsync();
    public async Task CommitTransactionAsync() => await baseRepository.CommitTransactionAsync();
    public async Task RollbackTransactionAsync(Exception ex) => await baseRepository.RollbackTransactionAsync(ex);
}