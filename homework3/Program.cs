using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#nullable enable

// Задача 1
public interface IEntity
{
    int Id { get; }
}

public class Repository<T> where T : class, IEntity
{
    private readonly Dictionary<int, T> _storage = new();

    public void Add(T item)
    {
        if (_storage.ContainsKey(item.Id))
            throw new InvalidOperationException($"Элемент с Id {item.Id} уже существует.");
        
        _storage.Add(item.Id, item);
    }

    public bool Remove(int id) => _storage.Remove(id);

    public T? GetById(int id) => _storage.TryGetValue(id, out var item) ? item : null;

    public IReadOnlyList<T> GetAll()
    {
        var list = new List<T>(_storage.Values);
        return new ReadOnlyCollection<T>(list);
    }

    public int Count => _storage.Count;

    public IReadOnlyList<T> Find(Predicate<T> predicate)
    {
        var result = new List<T>();
        foreach (var item in _storage.Values)
        {
            if (predicate(item))
                result.Add(item);
        }
        return new ReadOnlyCollection<T>(result);
    }
}

// Тестовые сущности
public class Product : IEntity
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }

    public Product(int id, string name, decimal price)
    {
        Id = id; Name = name; Price = price;
    }

    public override string ToString() => $"Product[{Id}] {Name} ({Price:C})";
}

public class User : IEntity
{
    public int Id { get; }
    public string Name { get; }

    public User(int id, string name) { Id = id; Name = name; }
    public override string ToString() => $"User[{Id}] {Name}";
}


// Задача 2
public static class CollectionUtils
{
    public static List<T> Distinct<T>(List<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var seen = new HashSet<T>();
        var result = new List<T>(source.Count);

        foreach (var item in source)
        {
            if (seen.Add(item))
                result.Add(item);
        }
        return result;
    }

    public static Dictionary<TKey, List<TValue>> GroupBy<TValue, TKey>(
        List<TValue> source,
        Func<TValue, TKey> keySelector) where TKey : notnull
    {
        if (source == null || keySelector == null)
            throw new ArgumentNullException(nameof(source));

        var dict = new Dictionary<TKey, List<TValue>>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (!dict.TryGetValue(key, out var groupList))
            {
                groupList = new List<TValue>();
                dict[key] = groupList;
            }
            groupList.Add(item);
        }
        return dict;
    }

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second,
        Func<TValue, TValue, TValue> conflictResolver) where TKey : notnull
    {
        if (first == null || second == null || conflictResolver == null)
            throw new ArgumentNullException();

        var result = new Dictionary<TKey, TValue>(first);

        foreach (var kvp in second)
        {
            if (result.TryGetValue(kvp.Key, out var existingValue))
            {
                result[kvp.Key] = conflictResolver(existingValue, kvp.Value);
            }
            else
            {
                result.Add(kvp.Key, kvp.Value);
            }
        }
        return result;
    }

    public static T MaxBy<T, TKey>(List<T> source, Func<T, TKey> selector)
        where TKey : IComparable<TKey>
    {
        if (source == null || source.Count == 0)
            throw new InvalidOperationException("Коллекция не должна быть пустой.");
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        var maxItem = source[0];
        var maxKey = selector(maxItem);

        for (int i = 1; i < source.Count; i++)
        {
            var currentKey = selector(source[i]);
            // CompareTo() возвращает > 0, если currentKey > maxKey
            if (currentKey.CompareTo(maxKey) > 0)
            {
                maxKey = currentKey;
                maxItem = source[i];
            }
        }
        return maxItem;
    }
}


// Проверки
class Program
{
    static void Main()
    {
        Console.WriteLine("=== ЗАДАЧА 1: Repository<T> ===\n");

        var productRepo = new Repository<Product>();
        productRepo.Add(new Product(1, "Laptop", 1500.00m));
        productRepo.Add(new Product(2, "Mouse", 50.00m));
        productRepo.Add(new Product(3, "Keyboard", 1200.00m));

        Console.WriteLine($"[Count] В репозитории: {productRepo.Count}");
        Console.WriteLine($"[GetById] Продукт с Id=1: {productRepo.GetById(1)}");

        var expensive = productRepo.Find(p => p.Price > 1000m);
        Console.WriteLine($"[Find] Продукты дороже 1000 ({expensive.Count} шт.):");
        foreach (var p in expensive) Console.WriteLine($"  - {p}");

        Console.Write("[Add Duplicate] Попытка добавить дубликат: ");
        try
        {
            productRepo.Add(new Product(1, "Fake Laptop", 999m));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Поймано исключение: {ex.Message}");
        }

        // Демонстрация переиспользования для другого типа
        var userRepo = new Repository<User>();
        userRepo.Add(new User(10, "Alice"));
        userRepo.Add(new User(11, "Bob"));
        Console.WriteLine($"[Cross-type] Репозиторий пользователей работает независимо. Count: {userRepo.Count}");


        Console.WriteLine("\n=== ЗАДАЧА 2: CollectionUtils ===\n");

        // 1. Distinct
        var ints = new List<int> { 1, 2, 2, 3, 4, 4, 5, 1 };
        var strs = new List<string> { "apple", "kiwi", "apple", "pear", "kiwi" };
        Console.WriteLine($"[Distinct] int:     {string.Join(", ", CollectionUtils.Distinct(ints))}");
        Console.WriteLine($"[Distinct] string:  {string.Join(", ", CollectionUtils.Distinct(strs))}");

        // 2. GroupBy
        var words = new List<string> { "cat", "dog", "elephant", "ant", "bear", "hippo" };
        var grouped = CollectionUtils.GroupBy(words, w => w.Length);
        Console.WriteLine($"[GroupBy] Группировка слов по длине:");
        foreach (var kvp in grouped)
            Console.WriteLine($"  Длина {kvp.Key}: {string.Join(", ", kvp.Value)}");

        // 3. Merge
        var dict1 = new Dictionary<string, int> { { "cat", 3 }, { "dog", 5 }, { "bird", 2 } };
        var dict2 = new Dictionary<string, int> { { "dog", 2 }, { "bat", 4 }, { "cat", 7 } };
        var merged = CollectionUtils.Merge(dict1, dict2, (v1, v2) => v1 + v2);
        Console.WriteLine($"[Merge] Сумма значений при совпадении ключей:");
        foreach (var kvp in merged)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");

        // 4. MaxBy
        var products = new List<Product>
        {
            new Product(1, "Pen", 1.50m),
            new Product(2, "Laptop", 1500.00m),
            new Product(3, "Notebook", 5.00m)
        };
        var maxProduct = CollectionUtils.MaxBy(products, p => p.Price);
        Console.WriteLine($"[MaxBy] Самый дорогой товар: {maxProduct}");
    }
}