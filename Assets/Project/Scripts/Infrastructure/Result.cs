using System;
using System.Collections.Generic;
using System.Linq;

namespace _3dt.Infrastructure
{
    public readonly struct Result : IResult
    {
        public bool IsSuccess { get; }
        public Exception Error { get; }

        private Result(bool isSuccess, Exception error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Ok() => new Result(true, null);

        public static Result Fail(Exception error) => new Result(false, error);

        public static Result Fail(string message) => new Result(false, new Exception(message));

        public static Result Fail(Exception error, Exception innerException) =>
            new Result(false, new Exception(error.Message, innerException));

        public void ThrowIfFailed()
        {
            if (!IsSuccess)
                throw Error;
        }

        public IEnumerable<Exception> GetAllErrors() => GetInnerExceptions(Error);

        internal static IEnumerable<Exception> GetInnerExceptions(Exception ex)
        {
            if (ex == null)
                return Enumerable.Empty<Exception>();

            var errors = new List<Exception>();
            while (ex != null)
            {
                errors.Add(ex);
                ex = ex.InnerException;
            }
            return errors;
        }
    }
}
