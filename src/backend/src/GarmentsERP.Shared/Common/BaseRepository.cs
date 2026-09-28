using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GarmentsERP.Shared.Common;

/// <summary>
/// Generic repository base implementation for Clean Architecture modules with EF Core.
/// Filters out soft-deleted entities by default.
/// </summary>
public class BaseRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : BaseEntity
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        // Interceptor or soft-delete will handle state
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .AnyAsync(predicate, ct);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
