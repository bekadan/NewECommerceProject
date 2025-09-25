namespace Core.Search.Abstractions;

public interface ISearchService<T>
{
    Task IndexAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<T>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
