using System.Linq.Expressions;

namespace MyProject.Infrastructures;

public interface IRepository<T> where T : class
{
    Task<ICollection<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate);
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    void DeleteMany(ICollection<T> entities);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}