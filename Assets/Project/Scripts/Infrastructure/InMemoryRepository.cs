using System.Collections.Generic;

public class InMemoryRepository<T> : IRepository<T>, IReadOnlyRepository<T>
{
    private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();

    public void Add(string key, T source)
    {
        _cache[key] = source;
    }

    public void Delete(string key)
    {
        _cache.Remove(key);
    }

    public T Get(string key)
    {
        return _cache[key];
    }

    public IEnumerable<T> GetAll()
    {
        return _cache.Values;
    }

    public void Update(string key, T source)
    {
        _cache[key] = source;
    }
}