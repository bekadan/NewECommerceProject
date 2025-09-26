namespace Core.Shared.Repositories;

public interface IQuery<T>
{
    IQueryable<T> Query();
}
