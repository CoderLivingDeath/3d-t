using System;
using UnityEngine;
using _3dt.Infrastructure;

public class ResourceConfigService : IConfigService
{
    private readonly IReadOnlyRepository<Config> _repository;

    public ResourceConfigService(IReadOnlyRepository<Config> repository)
    {
        _repository = repository;
    }

    public IResult<T> Get<T>(string keyOrPath)
    {
        var entry = _repository.Get(keyOrPath);
        if (entry == null)
            return Result<T>.Failure(new Exception($"Config with key '{keyOrPath}' not found"));

        if (entry is T result)
            return Result<T>.Success(result);

        return Result<T>.Failure(new Exception($"Config with key '{keyOrPath}' is not of type {typeof(T)}"));
    }
}
