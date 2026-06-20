using System.Collections.Generic;
using UnityEngine;

public class ResourceConfigRepository : IReadOnlyRepository<Object>
{
    private readonly Dictionary<string, Object> _cache = new();

    public Object? Get(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var resource = Resources.Load(key);
        if (resource == null)
            return null;

        _cache[key] = resource;
        return resource;
    }

    public IEnumerable<Object> GetAll()
    {
        return Resources.LoadAll("/.");
    }
}
