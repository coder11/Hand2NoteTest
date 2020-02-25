using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.Util
{
    public static class ObservableHelpers
    {
        public static ObservableAsPropertyHelper<TRet> ToPropertyOnMainThread<TObj, TRet>(this IObservable<TRet> item,
            TObj source, Expression<Func<TObj, TRet>> property, TRet initialValue = default,
            bool deferSubscription = false)
            where TObj : ReactiveObject
        {
            return item.ToPropertyEx(source, property, initialValue, deferSubscription, RxApp.MainThreadScheduler);
        }
        
        public static IObservable<TResult?> OfType<TResult, TSource>(this IObservable<TSource> item) where TResult : class
        {
            return item
                .Select(x => x as TResult)
                .Where(x => x != null);
        }

        public static IObservable<bool> BooleanAnd(this IObservable<bool> item, bool value)
        {
            return item.Select(x => x && value);
        }
        
        public static IObservable<bool> BooleanAnd(this IObservable<bool> item1, IObservable<bool> item2)
        {
            return item1.CombineLatest(item2, (x, y) => x && y);
        }
        
        public static IObservable<bool> Negate(this IObservable<bool> item)
        {
            return item.Select(x => !x);
        }
        
        public static IObservable<bool> TrueAfter<T>(this IObservable<T> item)
        {
            return item.Select(x => true)
                .StartWith(false)
                .DistinctUntilChanged();
        }
        
        public static IObservable<bool> TrueBefore<T>(this IObservable<T> item)
        {
            return item.TrueAfter().Negate();
        }
        
        public static IObservable<bool> TrueUntil<T1, T2>(this IObservable<T1> item1, IObservable<T2> item2)
        {
            return item1.Select(x => true)
                .Merge(item2.Select(x => false))
                .DistinctUntilChanged();
        }
    }
}