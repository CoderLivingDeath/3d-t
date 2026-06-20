using System.Collections;
using System.Collections.Generic;

public interface IReadOnlyRepository<T>
{
    T? Get(string key);

    IEnumerable<T> GetAll();
}
