using System.Collections.Generic;
using UnityEngine;

public class ResourceConfigRepository : IReadOnlyRepository<Config>
{
    private readonly Dictionary<string, Config> _cache = new();

    public Config Get(string key)
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var resource = Resources.Load<Config>(key);
        if (resource == null)
            return null;

        _cache[key] = resource;
        return resource;
    }

    public IEnumerable<Config> GetAll()
    {
        return Resources.LoadAll<Config>(".");
    }
}
