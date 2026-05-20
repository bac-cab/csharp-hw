namespace TaskHub.Services;

public class InMemoryRepository<T> : IRepository<T>
{
    private readonly List<T> _items = new();

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public bool Remove(Predicate<T> predicate)
    {
        var index = _items.FindIndex(predicate);
        if (index < 0)
        {
            return false;
        }

        _items.RemoveAt(index);
        return true;
    }

    public T? Find(Predicate<T> predicate) => _items.Find(predicate);

    public List<T> FindAll(Predicate<T> predicate) => _items.FindAll(predicate);

    public void ReplaceAll(IEnumerable<T> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }
}
