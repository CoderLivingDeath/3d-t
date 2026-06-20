using System;
using System.Collections.Generic;

namespace _3dt.Infrastructure
{
    public interface IResult
    {
        bool IsSuccess { get; }
        Exception Error { get; }
        void ThrowIfFailed();
        IEnumerable<Exception> GetAllErrors();
    }

    public interface IResult<T> : IResult
    {
        T Value { get; }
        T GetValue();
        T GetValueOrDefault(T defaultValue);
    }
}
