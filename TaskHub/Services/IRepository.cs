namespace TaskHub.Services;

public interface IRepository<T>
{
    IReadOnlyList<T> GetAll();
    void Add(T item);
    bool Remove(Predicate<T> predicate);
    T? Find(Predicate<T> predicate);
    List<T> FindAll(Predicate<T> predicate);
}
