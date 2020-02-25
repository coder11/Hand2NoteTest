using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
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
        
        public static IObservable<TResult> OfType<TResult, TSource>(this IObservable<TSource> item) where TResult : class
        {
            return item
                .Select(x => x as TResult)
                .Where(x => x != null);
        }
    }
}