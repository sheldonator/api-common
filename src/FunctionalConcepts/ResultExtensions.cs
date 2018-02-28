using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunctionalConcepts
{
    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this Option<T> option, string message) 
            where T : class
        {
            if (option.HasNoValue)
                return Result.Fail<T>(message).Log();

            return Result.Ok(option.Value);
        }

        public static Result<T> ToResult<T>(this Option<T> option, string message, string logMessage) 
            where T : class
        {
            if (option.HasNoValue)
                return Result.Fail<T>(message).Log(logMessage);

            return Result.Ok(option.Value);
        }

        public static async Task<Result<T>> OnSuccess<T>(this Task<Result<T>> result, Func<Result<T>, Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsSuccess)
                return await func.Invoke(actualResult);
            
            return actualResult;
        }

        public static async Task<Result<T>> OnSuccess<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
        {
            if (result.IsSuccess)
                return await func.Invoke(result);

            return result;
        }

        public static async Task<Result<K>> OnSuccess<T, K>(this Task<Result<T>> result, Func<T, Task<Result<K>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<K>(actualResult.Error).Log();

            return await func(actualResult.Value);
        }

        public static async Task<Result<K>> OnSuccess<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error).Log();

            return await func(result.Value);
        }

        public static async Task<Result<K>> OnSuccess<T, K>(this Task<Result<T>> result, Func<Task<K>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<K>(actualResult.Error).Log();

            return Result.Ok(await func());
        }

        public static Result<K> OnSuccess<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error).Log();

            return Result.Ok(func(result.Value));
        }

        public static Result<K> OnSuccess<T, K>(this Result<T> result, Func<T, Result<K>> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error).Log();

            return func(result.Value);
        }

        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
                action(result.Value);

            return result;
        }

        public static async Task<Result<T>> OnSuccess<T>(this Task<Result<T>> result, Action<T> action)
        {
            var actualResult = await result;

            if (actualResult.IsSuccess)
                action(actualResult.Value);

            return actualResult;
        }

        public static async Task<Result<T>> OnSuccess<T>(this Task<Result<T>> result, Func<T, Task> action)
        {
            var actualResult = await result;

            if (actualResult.IsSuccess)
                await action(actualResult.Value);

            return actualResult;
        }

        public static void OnFailure<T>(this Result<T> result, Action<string> action)
        {
            if (result.IsFailure)
                action(result.Error.Message);
        }

        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> result, Func<T, bool> predicate, string message)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (!predicate(actualResult.Value))
                return Result.Fail<T>(message, ErrorType.BadRequest).LogAsInfo();

            return actualResult;
        }
        
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> result, Func<T, bool> predicate, Func<T, string> message)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (!predicate(actualResult.Value))
                return Result.Fail<T>(message(actualResult.Value), ErrorType.BadRequest).LogAsInfo();

            return actualResult;
        }
        
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> result, Func<Result<T>, Task<bool>> predicate, string message)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;
            var p = await predicate(actualResult);
            return p ? actualResult : Result.Fail<T>(message, ErrorType.BadRequest).LogAsInfo();
        }

        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> result, Func<Result<T>, Task<bool>> predicate, Func<T, string> message)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;
            var p = await predicate(actualResult);
            return p ? actualResult : Result.Fail<T>(message(actualResult.Value), ErrorType.BadRequest).LogAsInfo();
        }

        public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> result, Func<T, Task<bool>> predicate, string message)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            var p = await predicate(actualResult.Value);
            if (!p)
                return Result.Fail<T>(message, ErrorType.BadRequest).LogAsInfo();

            return actualResult;
        }

        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string message)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Fail<T>(message, ErrorType.BadRequest).LogAsInfo();

            return result;
        }

        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Func<T, string> message)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Fail<T>(message(result.Value), ErrorType.BadRequest).LogAsInfo();

            return result;
        }
        
        public static async Task<Result<T>> EnsureOnCondition<T>(this Task<Result<T>> result, Func<T, bool> conditionPredicate, Func<T, bool> predicate, string message)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (conditionPredicate.Invoke(actualResult.Value) && !predicate(actualResult.Value))
                return Result.Fail<T>(message, ErrorType.BadRequest).LogAsInfo();

            return actualResult;
        }

        public static async Task<Result<K>> Map<T, K>(this Task<Result<T>> result, Func<T, K> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<K>(actualResult.Error).Log();

            return Result.Ok(func.Invoke(actualResult.Value));
        }

        public static async Task<Result<K>> Map<T, K>(this Task<Result<T>> result, Func<K> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<K>(actualResult.Error).Log();

            return Result.Ok(func.Invoke());
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error).Log();

            return Result.Ok(func.Invoke(result.Value));
        }

        public static Result<T> Map<T>(this Result result, Func<T> func)
        {
            if (result.IsFailure)
                return Result.Fail<T>(result.Error).Log();

            return Result.Ok(func.Invoke());
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<K> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error).Log();

            return Result.Ok(func.Invoke());
        }

        public static async Task<Result<T>> Map<T>(this Task<Result> result, Func<T> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<T>(actualResult.Error).Log();

            return Result.Ok(func.Invoke());
        }

        public static async Task<Result<K>> Map<T, K>(this Task<Result<T>> result, Func<T, K> func, string messageOverride)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<K>(messageOverride, actualResult.Error.Type, actualResult.Error.ValidationErrors).Log();

            return Result.Ok(func.Invoke(actualResult.Value));
        }

        public static async Task<Result<K>> Map<T, K>(this Task<Result<T>> result, Func<K> func, string messageOverride)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<K>(messageOverride, actualResult.Error.Type, actualResult.Error.ValidationErrors).Log();

            return Result.Ok(func.Invoke());
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func, string messageOverride)
        {
            if (result.IsFailure)
                return Result.Fail<K>(messageOverride, result.Error.Type, result.Error.ValidationErrors).Log();

            return Result.Ok(func.Invoke(result.Value));
        }

        public static Result<T> Map<T>(this Result result, Func<T> func, string messageOverride)
        {
            if (result.IsFailure)
                return Result.Fail<T>(messageOverride, result.Error.Type, result.Error.ValidationErrors).Log();

            return Result.Ok(func.Invoke());
        }

        public static Result<K> Map<T, K>(this Result<T> result, Func<K> func, string messageOverride)
        {
            if (result.IsFailure)
                return Result.Fail<K>(messageOverride, result.Error.Type, result.Error.ValidationErrors).Log();

            return Result.Ok(func.Invoke());
        }

        public static async Task<Result<T>> Map<T>(this Task<Result> result, Func<T> func, string messageOverride)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<T>(messageOverride, actualResult.Error.Type, actualResult.Error.ValidationErrors).Log();

            return Result.Ok(func.Invoke());
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, Func<T, bool> predicate, Func<Result<T>, Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (predicate(actualResult.Value))
                return await func.Invoke(actualResult);

            return actualResult;
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, Func<T, bool> predicate, Func<T, Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (predicate(actualResult.Value))
                return await func.Invoke(actualResult.Value);

            return actualResult;
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, Func<T, bool> predicate, Func<Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (predicate(actualResult.Value))
                return await func.Invoke();

            return actualResult;
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, Func<bool> predicate, Func<Result<T>, Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (predicate())
                return await func.Invoke(actualResult);

            return actualResult;
        }

        public static Result<T> OnCondition<T>(this Result<T> result, Func<T, bool> predicate, Action<T> action)
        {
            if (result.IsSuccess && predicate(result.Value))
                action(result.Value);
            
            return result;
        }

        public static Result<T> OnCondition<T>(this Result<T> result, Func<bool> predicate, Action<T> action)
        {
            if (result.IsSuccess && predicate())
                action(result.Value);
            
            return result;
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, Func<T, Task<bool>> predicate, Func<Result<T>, Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (await predicate(actualResult.Value))
                return await func.Invoke(actualResult);

            return actualResult;
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, bool predicate, Func<Task<T>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (predicate)
                return Result.Ok(await func.Invoke());

            return actualResult;
        }

        public static async Task<Result<T>> OnCondition<T>(this Task<Result<T>> result, Func<Task<bool>> predicate, Func<Result<T>, Task<Result<T>>> func)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (await predicate())
                return await func.Invoke(actualResult);

            return actualResult;
        }

        public static async Task<Result<T>> OverrideErrorMessage<T>(this Task<Result<T>> result, string errorMessageOverride)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return Result.Fail<T>(errorMessageOverride, actualResult.Error.Type, actualResult.Error.ValidationErrors).Log();

            return actualResult;
        }

        public static Result<T> OverrideErrorMessage<T>(this Result<T> result, string errorMessageOverride)
        {
            if (result.IsFailure)
                return Result.Fail<T>(errorMessageOverride, result.Error.Type, result.Error.ValidationErrors).Log();

            return result;
        }

        public static Result OverrideErrorMessage(this Result result, string errorMessageOverride)
        {
            if (result.IsFailure)
                return Result.Fail(new ResultError(errorMessageOverride, result.Error.Type, result.Error.ValidationErrors)).Log();

            return result;
        }

        public static Result<T> FailIfNoValue<T>(this Result<T> result, string errorMessage)
        {
            if (result.IsFailure)
                return result;

            if (!result.HasValue)
                return Result.Fail<T>(errorMessage, ErrorType.Unknown);

            return result;
        }

        public static async Task<Result<T>> FailIfNoValue<T>(this Task<Result<T>> result, string errorMessage)
        {
            var actualResult = await result;

            if (actualResult.IsFailure)
                return actualResult;

            if (!actualResult.HasValue)
                return Result.Fail<T>(errorMessage, ErrorType.Unknown);

            return actualResult;
        }
    
        public static Result<IEnumerable<T>> Concat<T>(this Result<IEnumerable<T>> source, Result<IEnumerable<T>> other)
        {
            if (source.IsFailure) return Result.Fail<IEnumerable<T>>(source.Error);
            if (other.IsFailure) return Result.Fail<IEnumerable<T>>(other.Error);

            var first = source.Value;
            return Result.Ok(first.Concat(other.Value));
        }


        public static Result<IEnumerable<T>> Concat<T>(this Result<IEnumerable<T>> source, Task<IEnumerable<T>> otherTask)
        {
            if (source.IsFailure) return Result.Fail<IEnumerable<T>>(source.Error);

            var first = source.Value;
            var result = otherTask.GetAwaiter().GetResult();
            return Result.Ok(first.Concat(result));
        }

        public static Result<IEnumerable<T>> ApplySorting<T>(this Result<IEnumerable<T>> source, Func<IEnumerable<T>, IOrderedEnumerable<T>> predicate)
        {
            if (source.IsFailure) return Result.Fail<IEnumerable<T>>(source.Error);
            var sorted = predicate(source.Value) as IEnumerable<T>;
            return Result.Ok(sorted);
        }
        public static Result<IEnumerable<T>> ApplySorting<T>(this Result<IEnumerable<T>> source, Func<IEnumerable<T>, IEnumerable<T>> predicate)
        {
            if (source.IsFailure) return Result.Fail<IEnumerable<T>>(source.Error);
            var sorted = predicate(source.Value);
            return Result.Ok(sorted);
        }
        public static Result<IEnumerable<K>> Select<T, K>(this Result<IEnumerable<T>> source, Func<T, K> predicate)
        {
            if (source.IsFailure) return Result.Fail<IEnumerable<K>>(source.Error);
            var mapped = source.Value.Select(predicate);
            return Result.Ok(mapped);
        }
    }
}
