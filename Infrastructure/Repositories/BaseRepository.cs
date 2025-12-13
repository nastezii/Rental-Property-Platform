using System.Linq.Expressions;
using Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories.BaseRepository;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet
            .AsQueryable()
            .AsNoTracking();
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> expression)
    {
        var response = await GetAll()
            .SingleOrDefaultAsync(expression);

        return response ?? throw new NullReferenceException($"{typeof(T).Name} not found");
    }

    public async Task<T> GetByIdAsync(int id)
    {
        var response = await GetAll()
            .SingleOrDefaultAsync(x => x.Id == id);

        return response ?? throw new NullReferenceException($"{typeof(T).Name} not found");
    }

    public async Task<T> AddAsync(T entity)
    {
        if (entity is TrackingEntity trackingEntity)
            trackingEntity.DateAdded = DateTime.UtcNow;

        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();

        foreach (var entity in entityList)
        {
            if (entity is TrackingEntity trackingEntity)
                trackingEntity.DateAdded = DateTime.UtcNow;
        }

        await _dbSet.AddRangeAsync(entityList);
        await _context.SaveChangesAsync();
        return entityList;
    }

    public async Task<T> UpdateAsync(int id, T entity)
    {
        var existingEntity = await GetByIdAsyncTracking(id);
        entity.Id = id;

        if (entity is TrackingEntity trackingEntity)
            trackingEntity.DateAdded = DateTime.UtcNow;

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return existingEntity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
            _context.Set<T>().Attach(entity);

        entry.State = EntityState.Modified;
        _context.Entry(entity).CurrentValues.SetValues(entity);

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();
        _dbSet.UpdateRange(entityList);

        await _context.SaveChangesAsync();
        return entityList;
    }

    public async Task<T> DeleteAsync(int id)
    {
        var entity = await GetByIdAsyncTracking(id);
        _dbSet.Remove(entity);

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> DeleteAsync(Expression<Func<T, bool>> expression)
    {
        var entity = await GetByIdAsyncTracking(expression);
        _dbSet.Remove(entity);

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<T>> DeleteRangeAsync(IEnumerable<T> entities)
    {
        var deleteRangeAsync = entities.ToList();
        _context.RemoveRange(deleteRangeAsync);

        await _context.SaveChangesAsync();
        return deleteRangeAsync;
    }

    public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction has not been started");

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackTransactionAsync(ex);
            throw new Exception("The row was modified by another user, try again");
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(ex);
        }
        finally
        {
            await _transaction.DisposeAsync();
        }
    }

    public async Task RollbackTransactionAsync(Exception ex)
    {
        if (_transaction == null)
            return;

        try
        {
            await _transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }
        catch (Exception)
        {
            throw ex;
        }
    }

    private async Task<T> GetByIdAsyncTracking(int id)
    {
        var response = await _dbSet
            .AsQueryable()
            .SingleOrDefaultAsync(x => x.Id == id);

        return response ?? throw new NullReferenceException($"{typeof(T).Name} not found");
    }

    private async Task<T> GetByIdAsyncTracking(Expression<Func<T, bool>> expression)
    {
        var response = await _dbSet
            .AsQueryable()
            .SingleOrDefaultAsync(expression);

        return response ?? throw new NullReferenceException($"{typeof(T).Name} not found");
    }
}