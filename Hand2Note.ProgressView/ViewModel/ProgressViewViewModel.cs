// ReSharper disable UnassignedGetOnlyAutoProperty

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Hand2Note.ProgressView.Util;
using Hand2Note.ProgressView.ViewModel.Progress;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class ProgressViewModel : ReactiveObject
    {
        const double SpeedDeltaSecs = 1;
        private readonly TimeSpan SpeedDelta = TimeSpan.FromSeconds(SpeedDeltaSecs);

        private readonly TimeSpan TextRefreshRate = TimeSpan.FromSeconds(1);
        
        public ProgressViewModel(IObservable<IProgressNotification> notifications, ProgressViewModelConfig config, Action onStart, Action onPause, Action onResume)
        {
            InitializeObservables(notifications, config, onStart, onPause, onResume);
        }

        private ReactiveCommand<Unit, Unit> _startCommand;
        private ReactiveCommand<Unit, Unit> _restartCommand;
        private ReactiveCommand<Unit, Unit> _pauseCommand;
        private ReactiveCommand<Unit, Unit> _resumeCommand;

        public int ProgressMaxValue { [ObservableAsProperty] get; }

        public int Progress { [ObservableAsProperty] get; }
        
        public bool DisplayAsProgressless { [ObservableAsProperty] get; }

        public string Speed { [ObservableAsProperty] get; }
        
        public bool SpeedVisible { [ObservableAsProperty] get; }
        
        public string ProgressText { [ObservableAsProperty] get; }
        
        public bool ProgressTextVisible { [ObservableAsProperty] get; }
        
        public string RemainingTime { [ObservableAsProperty] get; }
        
        public bool RemainingTimeVisible { [ObservableAsProperty] get; }
        
        public string CommandButtonText { [ObservableAsProperty] get; }

        public string Caption { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> Command { [ObservableAsProperty] get; }
        
        private void InitializeObservables(IObservable<IProgressNotification> notifications, ProgressViewModelConfig config, Action onStart, Action onPause, Action onResume)
        {
            notifications = notifications.StartWith(new ProgressNotification(
                0,
                null,
                1,
                false,
                false,
                false,
                true,
                true,
                true,
                false,
                string.Empty));

            var startCommandFired = this.WhenAnyObservable(x => x._startCommand);

            var isRunning = startCommandFired
                .Select(x => true)
                .Merge(notifications
                    .Where(x => x.HasFinished)
                    .Select(x => false))
                .StartWith(false);

            var wasNeverRun = startCommandFired
                .Select(x => true)
                .StartWith(true);

            var canStartExecute = isRunning
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => !x && onStart != null);

            var canPauseExecute = notifications.Select(x => x.AllowPause && onPause != null)
                .ObserveOn(RxApp.MainThreadScheduler);

            var canResumeExecute = notifications.Select(x => x.AllowResume && onResume != null)
                .ObserveOn(RxApp.MainThreadScheduler);

            _startCommand = ReactiveCommand.Create(onStart, canStartExecute);
            _restartCommand = _startCommand;
            _pauseCommand = ReactiveCommand.Create(onPause, canPauseExecute);
            _resumeCommand = ReactiveCommand.Create(onResume, canResumeExecute);

            var currentlyAvailableCommand =
                Observable.CombineLatest(notifications, isRunning, wasNeverRun,
                    (stateInfo, isRunning_, wasNeverRun_) =>
                    {
                        if (isRunning_ && stateInfo.AllowPause)
                            return new
                            {
                                Command = _pauseCommand,
                                Caption = config.PauseButtonText
                            };

                        if (isRunning_ && stateInfo.AllowResume)
                            return new
                            {
                                Command = _resumeCommand,
                                Caption = config.ResumeButtonText
                            };

                        if (wasNeverRun_)
                            return new
                            {
                                Command = _startCommand,
                                Caption = config.StartButtonText
                            };

                        return new
                        {
                            Command = _startCommand,
                            Caption = config.RestartButtonText
                        };
                    });

            currentlyAvailableCommand
                .Select(x => x.Command)
                .ToPropertyOnMainThread(this, x => x.Command);

            currentlyAvailableCommand
                .Select(x => x.Caption)
                .ToPropertyOnMainThread(this, x => x.CommandButtonText);

            notifications.Select(x => x.Caption)
                .ToPropertyOnMainThread(this, x => x.Caption);

            notifications.Select(x => x.Progress)
                .ToPropertyOnMainThread(this, x => x.Progress);

            notifications.Select(x => x.ProgressMaxValue)
                .ToPropertyOnMainThread(this, x => x.ProgressMaxValue);

            notifications.Select(x => x.DisplayAsProgressLess)
                .ToPropertyOnMainThread(this, x => x.DisplayAsProgressless);

            var consecutiveProgressNotifications = notifications
                .Select(x => new
                {
                    IsProgressless = x.DisplayAsProgressLess,
                    x.ProgressIncrement,
                    TotalProgress = x.Progress,
                    TotalBytesToDownload = x.ProgressMaxValue
                })
                .Scan(new
                {
                    IsProgressless = true,
                    ProgressIncrement = (int?) null,
                    TotalProgress = 0,
                    TotalBytesToDownload = 0,
                }, (acc, cur) => { return cur; })
                .Where(x => !x.IsProgressless && x.ProgressIncrement.HasValue)
                .Select(x => new
                {
                    x.ProgressIncrement.Value,
                    x.TotalProgress,
                    x.TotalBytesToDownload
                });

            var speed = consecutiveProgressNotifications
                .Buffer(SpeedDelta)
                .Select(x => x.Sum(y => y.Value) / SpeedDeltaSecs);

            var remaining = consecutiveProgressNotifications
                .Select(x => x.TotalBytesToDownload - x.TotalProgress);

            var remainingTime = Observable.CombineLatest(
                remaining,
                speed,
                (r, s) => r / s);

            speed
                .Select(x =>
                {
                    if (Double.IsInfinity(x) || Double.IsNaN(x))
                    {
                        return string.Format(config.SpeedTextTemplate, "?");
                    }

                    var rounded = (int) Math.Round(x);
                    var text = config.Units.GetPresentableText(rounded);
                    return string.Format(config.SpeedTextTemplate, text);
                })
                .ToPropertyOnMainThread(this, x => x.Speed);

            consecutiveProgressNotifications
                .Select(x =>
                {
                    return string.Format(config.ProgressTextTemplate,
                        config.Units.GetPresentableText(x.TotalProgress),
                        config.Units.GetPresentableText(x.TotalBytesToDownload));
                })
                .ToPropertyOnMainThread(this, x => x.ProgressText);

            remainingTime
                .Select(x =>
                {
                    if (Double.IsInfinity(x) || Double.IsNaN(x))
                    {
                        return string.Format(config.RemainingTimeTextTemplate, "?");
                    }

                    var rounded = Math.Round(x);
                    var ts = TimeSpan.FromSeconds(rounded);
                    return string.Format(config.RemainingTimeTextTemplate, ts);
                })
                .Sample(TextRefreshRate)
                .ToPropertyOnMainThread(this, x => x.RemainingTime);

            notifications
                .Select(x => !x.HideSpeed)
                .ToPropertyOnMainThread(this, x => x.SpeedVisible);

            notifications
                .Select(x => !x.HideProgressText)
                .ToPropertyOnMainThread(this, x => x.ProgressTextVisible);

            notifications
                .Select(x => !x.HideRemainingTime)
                .ToPropertyOnMainThread(this, x => x.RemainingTimeVisible);
        }
    }
}