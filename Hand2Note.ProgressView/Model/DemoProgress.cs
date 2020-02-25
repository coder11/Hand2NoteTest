using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Hand2Note.ProgressView.ViewModel.Progress;
using ReactiveUI;

namespace Hand2Note.ProgressView.Model
{
    public class DemoProgressOperation
    {
        private readonly (IProgressNotification, int)[] _notifications;
        private readonly Subject<IProgressNotification> _subject;
        
        private int _i = 0;
        private CancellationTokenSource _token;
        
        public DemoProgressOperation(int timeoutMs, IEnumerable<IProgressNotification> notifications)
        {
            _notifications = notifications.Select(x => (x, timeoutMs)).ToArray();
            
            _subject = new Subject<IProgressNotification>();
        }
        
        public DemoProgressOperation(params (IProgressNotification notification, int timeoutMs)[] notifications)
        {
            _notifications = notifications.ToArray();
            _subject = new Subject<IProgressNotification>();
        }

        public string PausedCaption { get; set; } = "Paused";
        
        public string FinishedCaption { get; set; } = "Finished";

        public IObservable<IProgressNotification> Notifications => _subject.ObserveOn(RxApp.TaskpoolScheduler);

        public void Start()
        {
            _i = 0;
            Resume();
        }
        
        public void Resume()
        {
            _token = new CancellationTokenSource();
            Task.Run(() =>
            {
                do
                {
                    _subject.OnNext(_notifications[_i].Item1);
                    Task.Delay(_notifications[_i].Item2).Wait();
                    _i++;
                } while (_i < _notifications.Length && !_token.IsCancellationRequested);

                if (!_token.IsCancellationRequested)
                {
                    _subject.OnNext(new FinishNotification(0, FinishedCaption));
                }
                
            }, _token.Token);
        }

        public void Pause()
        {
            _token.Cancel();
            _subject.OnNext(new PausedNotification(0, 0, PausedCaption));
        }
    }
}