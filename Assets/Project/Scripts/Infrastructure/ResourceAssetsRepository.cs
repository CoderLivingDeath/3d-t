using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceAssetsRepository : IRepository<UnityEngine.Object>, IReadOnlyRepository<UnityEngine.Object>
{
    public void Add(string key, UnityEngine.Object source)
    {
        throw new NotImplementedException();
    }

    public void Delete(string key)
    {
        throw new NotImplementedException();
    }

    public UnityEngine.Object Get(string key)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<UnityEngine.Object> GetAll()
    {
        throw new NotImplementedException();
    }

    public void Update(string key, UnityEngine.Object source)
    {
        throw new NotImplementedException();
    }
}