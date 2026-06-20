using System;
using System.Collections.Generic;

namespace _3dt.Infrastructure
{
    public readonly struct Result<T> : IResult<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public Exception Error { get; }

        private Result(bool isSuccess, T value, Exception error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);

        public static Result<T> Failure(Exception error) => new Result<T>(false, default, error);

        public static Result<T> Failure(string message) =>
            new Result<T>(false, default, new Exception(message));

        public static Result<T> Failure(string message, Exception innerException) =>
            new Result<T>(false, default, new Exception(message, innerException));

        public static implicit operator Result<T>(T value) => Result<T>.Success(value);

        public static implicit operator T(Result<T> result)
        {
            result.ThrowIfFailed();
            return result.Value;
        }

        public T GetValue() => ThrowIfFailedInternal();

        public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value : defaultValue;

        public void ThrowIfFailed() => ThrowIfFailedInternal();

        private T ThrowIfFailedInternal()
        {
            if (!IsSuccess)
                throw Error;
            return Value;
        }

        public IEnumerable<Exception> GetAllErrors() => Result.GetInnerExceptions(Error);
    }
}
