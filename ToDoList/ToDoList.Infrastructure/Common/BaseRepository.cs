using Microsoft.EntityFrameworkCore;
using ToDoList.Application.Repositories;
using ToDoList.Domain.Common;
using ToDoList.Infrastructure.Context;

namespace ToDoList.Infrastructure.Common;

public class BaseRepository<T>(ApiDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly ApiDbContext _context = context;

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Set<T>().AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Set<T>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<T> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
}