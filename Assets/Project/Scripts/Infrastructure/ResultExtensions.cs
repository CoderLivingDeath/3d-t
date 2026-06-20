using System;
using System.Collections.Generic;
using System.Linq;

namespace _3dt.Infrastructure
{
    public static class ResultExtensions
    {
        public static Result IfThen<T>(this IResult<T> result, Action<T> action)
        {
            if (result.IsSuccess)
                action(result.Value);
            return result.IsSuccess ? Result.Ok() : Result.Fail(result.Error);
        }

        public static Result IfThen(this IResult result, Action action)
        {
            if (!result.IsSuccess)
                return Result.Fail(result.Error);
            action();
            return Result.Ok();
        }

        public static Result<U> Map<T, U>(this IResult<T> result, Func<T, U> mapper)
        {
            if (!result.IsSuccess)
                return Result<U>.Failure(result.Error);
            return Result<U>.Success(mapper(result.Value));
        }

        public static Result<U> Map<U>(this IResult result, Func<U> mapper)
        {
            if (!result.IsSuccess)
                return Result<U>.Failure(result.Error);
            return Result<U>.Success(mapper());
        }

        public static Result<U> FlatMap<T, U>(this IResult<T> result, Func<T, Result<U>> mapper)
        {
            if (!result.IsSuccess)
                return Result<U>.Failure(result.Error);
            return mapper(result.Value);
        }

        public static Result<U> FlatMap<U>(this IResult result, Func<Result<U>> mapper)
        {
            if (!result.IsSuccess)
                return Result<U>.Failure(result.Error);
            return mapper();
        }

        public static Result And(this IResult first, IResult second)
        {
            if (!first.IsSuccess)
                return Result.Fail(first.Error);
            if (!second.IsSuccess)
                return Result.Fail(second.Error, first.Error);
            return Result.Ok();
        }

        public static Result<U> And<T, U>(this IResult<T> first, IResult<U> second)
        {
            if (!first.IsSuccess)
                return Result<U>.Failure(first.Error);
            if (!second.IsSuccess)
                return Result<U>.Failure(second.Error.Message, first.Error);
            return Result<U>.Success(second.Value);
        }

        public static IResult<T> Or<T>(this IResult<T> result, IResult<T> other)
        {
            return result.IsSuccess ? result : other;
        }

        public static IResult Or(this IResult result, IResult other)
        {
            return result.IsSuccess ? result : other;
        }

        public static void Match<T>(
            this IResult<T> result,
            Action<T> onSuccess,
            Action<Exception> onFailure
        )
        {
            if (result.IsSuccess)
                onSuccess(result.Value);
            else
                onFailure(result.Error);
        }

        public static void Match(this IResult result, Action onSuccess, Action<Exception> onFailure)
        {
            if (result.IsSuccess)
                onSuccess();
            else
                onFailure(result.Error);
        }

        public static IEnumerable<string> GetErrorMessages(this IResult result) =>
            result.GetAllErrors().Select(e => e.Message);

        public static string GetErrorSummary(this IResult result) =>
            string.Join("\n", result.GetErrorMessages());
    }

    public static class ResultTExtensions
    {
        public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
        {
            if (!result.IsSuccess)
                return Result<U>.Failure(result.Error);
            return Result<U>.Success(mapper(result.Value));
        }

        public static Result<U> FlatMap<T, U>(this Result<T> result, Func<T, Result<U>> mapper)
        {
            if (!result.IsSuccess)
                return Result<U>.Failure(result.Error);
            return mapper(result.Value);
        }

        public static Result And<T>(this Result<T> first, Result second)
        {
            if (!first.IsSuccess)
                return Result.Fail(first.Error);
            if (!second.IsSuccess)
                return Result.Fail(second.Error, first.Error);
            return Result.Ok();
        }

        public static Result<T> Or<T>(this Result<T> result, Result<T> other)
        {
            return result.IsSuccess ? result : other;
        }
    }
}
