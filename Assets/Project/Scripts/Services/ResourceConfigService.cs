using System;
using UnityEngine;
using _3dt.Infrastructure;

public class ResourceConfigService : IConfigService
{
    private readonly ResourceConfigRepository repository;

    public ResourceConfigService()
    {
        repository = new();
    }

    public IResult<T> Get<T>(string keyOrPath)
    {
        var entry = repository.Get(keyOrPath);
        if (entry == null)
            return Result<T>.Failure(new Exception($"Config with key '{keyOrPath}' not found"));

        if (entry is T result)
            return Result<T>.Success(result);

        return Result<T>.Failure(new Exception($"Config with key '{keyOrPath}' is not of type {typeof(T)}"));
    }
}
