using System;
using System.Linq.Expressions;
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
    }
}