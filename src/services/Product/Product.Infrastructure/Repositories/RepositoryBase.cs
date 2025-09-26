using Core.Shared.Abstractions;
using Core.Shared.Primitives;
using Core.Shared.Repositories;
using Core.Shared.Repositories.Dynamic;
using Core.Shared.Repositories.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Product.Infrastructure.Repositories;

public class EfRepository<TEntity, TEntityId> : IAsyncRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public EfRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query()
    {
        return _dbSet;
    }

    #region Get / Query

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!enableTracking) query = query.AsNoTracking();
        if (!withDeleted && typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(TEntity)))
            query = query.Where(e => !((ISoftDeletableEntity)e).Deleted);

        if (include != null) query = include(query);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IPaginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!enableTracking) query = query.AsNoTracking();
        if (!withDeleted && typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(TEntity)))
            query = query.Where(e => !((ISoftDeletableEntity)e).Deleted);

        if (predicate != null) query = query.Where(predicate);
        if (include != null) query = include(query);
        if (orderBy != null) query = orderBy(query);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(index * size).Take(size).ToListAsync(cancellationToken);

        return new Paginate<TEntity>(items, totalCount, index, size);
    }

    public async Task<IPaginate<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamicQuery,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!enableTracking) query = query.AsNoTracking();
        if (!withDeleted && typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(TEntity)))
            query = query.Where(e => !((ISoftDeletableEntity)e).Deleted);

        if (predicate != null) query = query.Where(predicate);
        if (include != null) query = include(query);
        if (dynamicQuery != null) query = query.ToDynamic(dynamicQuery);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(index * size).Take(size).ToListAsync(cancellationToken);

        return new Paginate<TEntity>(items, totalCount, index, size);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!withDeleted && typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(TEntity)))
            query = query.Where(e => !((ISoftDeletableEntity)e).Deleted);

        if (predicate != null) query = query.Where(predicate);
        if (include != null) query = include(query);

        return await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Add / Update / Delete

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, CancellationToken cancellationToken = default)
    {
        if (!permanent && entity is ISoftDeletableEntity deletable)
        {
            deletable.MarkDeleted(); // domain method should also call SetModified()
            await UpdateAsync(entity, cancellationToken);
        }
        else
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false, CancellationToken cancellationToken = default)
    {
        if (permanent)
        {
            _dbSet.RemoveRange(entities);
        }
        else
        {
            foreach (var entity in entities.OfType<ISoftDeletableEntity>())
            {
                entity.MarkDeleted(); // domain handles timestamp
            }

            _dbSet.UpdateRange(entities);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entities;
    }

    #endregion
}