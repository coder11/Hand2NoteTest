using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Hand2Note.ProgressView.ViewModel.Progress;
using ReactiveUI;

namespace Hand2Note.ProgressView.Model
{
    public class DemoProgressOperation
    {
        private object syncObject = new object();
        private bool _isRunning = false;

        public DemoProgressOperation(int timeoutMs, IEnumerable<IProgressNotification> progress)
        {
            var vals = progress.ToObservable()
                .Select(x =>
                {
                    if (_isRunning)
                        return x;

                    return x;
                })
                .Distinct()
                .Append(new FinishNotification(progress.Count(), "Done"));

            var intervals = Observable.Interval(TimeSpan.FromMilliseconds(timeoutMs), RxApp.TaskpoolScheduler)
                .Where(x => _isRunning);

            Notifications = Observable.Zip(vals, intervals, (v, t) => v);
        }
        
        public IObservable<IProgressNotification> Notifications { get; }

        public void Start()
        {
            lock (syncObject)
            {
                if (!_isRunning)
                {
                    _isRunning = true;
                }
            }
        }

        public void Pause()
        {
            lock (syncObject)
            {
                if (_isRunning)
                {
                    _isRunning = false;
                }
            }
        }
    }
}