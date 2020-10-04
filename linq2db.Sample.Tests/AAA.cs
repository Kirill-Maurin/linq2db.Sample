﻿using System;
using System.Reactive;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NSubstitute.ExceptionExtensions;

namespace linq2db.Sample.Tests
{
    public static class ExceptionExtensions
    {
        public static Unit Throw(this Exception e) => throw e;
    }
    
    public static class AAA
    {
        public static ArrangeResult<T, Unit> Arrange<T>(this T @object, Action<T> action)
        {
            action(@object);
            return new ArrangeResult<T, Unit>(@object, default);
        }

        public static ArrangeResult<T, Unit> Arrange<T>(T @object) 
            => new ArrangeResult<T, Unit>(@object, default);

        public static ArrangeResult<T, TMock> Arrange<T, TMock>(this TMock mock, Func<TMock, T> @object) 
            => new ArrangeResult<T, TMock>(@object(mock), mock);

        public static ActResult<T, TMock> Act<T, TMock>(this ArrangeResult<T, TMock> arrange, Action<T> act)
        {
            try
            {
                act(arrange.Object);
                return new ActResult<T, TMock>(arrange.Object, arrange.Mock, default);
            }
            catch (Exception e)
            {
                return new ActResult<T, TMock>(arrange.Object, arrange.Mock, e);
            }
        }

        public static ActResult<TResult, TMock> Act<T, TMock, TResult>(this ArrangeResult<T, TMock> arrange, Func<T, TResult> act)
        {
            try
            {
                return new ActResult<TResult, TMock>(act(arrange.Object), arrange.Mock, default);
            }
            catch (Exception e)
            {
                return new ActResult<TResult, TMock>(default, arrange.Mock, e);
            }
        }

        public static void Assert<T, TMock>(this ActResult<T, TMock> act, [InstantHandle] Action<T> assert)
        {
            act.Exception?.Throw();
            assert(act.Object);
        }

        public static void Assert<T, TMock>(this ActResult<T, TMock> act, [InstantHandle] Action<T, TMock> assert)
        {
            act.Exception?.Throw();
            assert(act.Object, act.Mock);
        }

        public static Task<ArrangeResult<T, Unit>> ArrangeAsync<T>(T @object)
            => Task.FromResult(new ArrangeResult<T, Unit>(@object, default));

        public static async Task<ActResult<TResult, TMock>> Act<T, TMock, TResult>(this Task<ArrangeResult<T, TMock>> arrange, Func<T, Task<TResult>> act)
        {
            var a = await arrange;
            try
            {
                return new ActResult<TResult, TMock>(await act(a.Object), a.Mock, default);
            }
            catch (Exception e)
            {
                return new ActResult<TResult, TMock>(default, a.Mock, e);
            }
        }

        public static async Task Assert<T, TMock>(this Task<ActResult<T, TMock>> act, Func<T, Task> assert)
        {
            var result = await act;
            await assert(result.Object);
        }

        public readonly struct ArrangeResult<T, TMock>
        {
            internal ArrangeResult(T @object, TMock mock) => (Object, Mock) = (@object, mock);
            internal T Object { get; }
            internal TMock Mock { get; }
        }

        public readonly struct ActResult<T, TMock>
        {
            internal ActResult(T @object, TMock mock, Exception exception)
                => (Object, Mock, Exception) = (@object, mock, exception);
            internal T Object { get; }
            internal TMock Mock { get; }
            internal Exception Exception { get; }
        }
    }
}