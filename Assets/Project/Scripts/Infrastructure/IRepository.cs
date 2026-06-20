using System.Collections.Generic;

public interface IRepository<T>
{
    void Add(string key, T source);
    T? Get(string key);
    IEnumerable<T> GetAll();
    void Update(string key, T source);
    void Delete(string key);
}
