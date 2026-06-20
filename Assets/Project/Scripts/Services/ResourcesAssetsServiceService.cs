using System;
using UnityEngine;
using _3dt.Infrastructure;

public class ResourcesAssetsServiceService
{
    private readonly ResourceAssetsRepository _resourceAssetsRepository;

    public ResourcesAssetsServiceService(ResourceAssetsRepository resourceAssetsRepository)
    {
        _resourceAssetsRepository = resourceAssetsRepository;
    }

    public IResult<T> Get<T>(string keyOrPath)
    {
        var entry = _resourceAssetsRepository.Get(keyOrPath);
        if (entry == null)
            return Result<T>.Failure(new Exception($"Asset with key '{keyOrPath}' not found"));

        if (entry is T result)
            return Result<T>.Success(result);

        return Result<T>.Failure(new Exception($"Asset with key '{keyOrPath}' is not of type {typeof(T)}"));
    }
}