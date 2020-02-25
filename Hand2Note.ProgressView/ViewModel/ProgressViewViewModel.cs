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
        private const double SpeedDeltaSecs = 1;
        private TimeSpan SpeedDelta => TimeSpan.FromSeconds(SpeedDeltaSecs);

        private readonly TimeSpan TextRefreshRate = TimeSpan.FromSeconds(1);
        
        public ProgressViewModel(IObservable<IProgressNotification> notifications, ProgressViewModelConfig config, Action start = null, Action restart = null, Action pause = null, Action resume = null)
        {
            InitializeObservables(notifications, config, start, restart, pause, resume);
        }

        private ReactiveCommand<Unit, Unit> _startCommand;
        private ReactiveCommand<Unit, Unit> _restartCommand;
        private ReactiveCommand<Unit, Unit> _pauseCommand;
        private ReactiveCommand<Unit, Unit> _resumeCommand;

        public int ProgressMaxValue { [ObservableAsProperty] get; }

        public int Progress { [ObservableAsProperty] get; }
        
        public bool DisplayAsProgressLess { [ObservableAsProperty] get; }

        public string Speed { [ObservableAsProperty] get; }
        
        public bool SpeedVisible { [ObservableAsProperty] get; }
        
        public string ProgressText { [ObservableAsProperty] get; }
        
        public bool ProgressTextVisible { [ObservableAsProperty] get; }
        
        public string RemainingTime { [ObservableAsProperty] get; }
        
        public bool RemainingTimeVisible { [ObservableAsProperty] get; }
        
        public string CommandButtonText { [ObservableAsProperty] get; }

        public string Caption { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> Command { [ObservableAsProperty] get; }
        
        private void InitializeObservables(IObservable<IProgressNotification> notifications, ProgressViewModelConfig config, Action start, Action restart, Action pause, Action resume)
        {
            var disableStart = start == null;
            var disableRestart = restart == null;
            var disablePause = pause == null || resume == null;

            start ??= ActionHelpers.Empty;
            restart ??= ActionHelpers.Empty;
            pause ??= ActionHelpers.Empty;
            resume ??= ActionHelpers.Empty;
            
            notifications = notifications.StartWith(new BaseProgressNotification(
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

            var startCommandFired = this.WhenAnyObservable(x => x._startCommand, x => x._restartCommand)
                .Select(_ => true);

            var isRunning = startCommandFired
                .Select(x => true)
                .Merge(notifications
                    .Where(x => x.HasFinished)
                    .Select(x => false))
                .StartWith(false);

            var wasNeverRun = startCommandFired
                .Select(x => false)
                .StartWith(true);
                
            var canStartExecute = wasNeverRun
                .Select(x => x && !disableStart)
                .ObserveOn(RxApp.MainThreadScheduler);

            var canRestartExecute = isRunning
                .Select(x => !x && !disableRestart)
                .ObserveOn(RxApp.MainThreadScheduler);
                

            var canPauseExecute = notifications.Select(x => x.AllowPause && !disablePause)
                .ObserveOn(RxApp.MainThreadScheduler);

            var canResumeExecute = notifications.Select(x => x.AllowResume && !disablePause)
                .ObserveOn(RxApp.MainThreadScheduler);

            _startCommand = ReactiveCommand.Create(start, canStartExecute);
            _restartCommand = ReactiveCommand.Create(restart, canRestartExecute);
            _pauseCommand = ReactiveCommand.Create(pause, canPauseExecute);
            _resumeCommand = ReactiveCommand.Create(resume, canResumeExecute);

            var currentlyAvailableCommand =
                Observable.CombineLatest(notifications, isRunning, wasNeverRun,
                    (stateInfo, isRunning_, wasNeverRun_) =>
                    {
                        if (wasNeverRun_)
                            return new
                            {
                                Command = _startCommand,
                                Caption = config.StartButtonText
                            };

                        if (!isRunning_)
                            return new
                            {
                                Command = _restartCommand,
                                Caption = config.RestartButtonText
                            };

                        if (stateInfo.AllowPause)
                            return new
                            {
                                Command = _pauseCommand,
                                Caption = config.PauseButtonText
                            };
                        
                        if (stateInfo.AllowResume)
                            return new
                            {
                                Command = _resumeCommand,
                                Caption = config.ResumeButtonText
                            };

                        return null;
                    });

            currentlyAvailableCommand
                .Where(x => x != null)
                .Select(x => x.Command)
                .ToPropertyOnMainThread(this, x => x.Command);

            currentlyAvailableCommand
                .Where(x => x != null)
                .Select(x => x.Caption)
                .ToPropertyOnMainThread(this, x => x.CommandButtonText, config.StartButtonText);

            notifications.Select(x => x.Caption)
                .ToPropertyOnMainThread(this, x => x.Caption);

            notifications.Select(x => x.Progress)
                .ToPropertyOnMainThread(this, x => x.Progress);

            notifications.Select(x => x.ProgressMaxValue)
                .ToPropertyOnMainThread(this, x => x.ProgressMaxValue);

            notifications.Select(x => x.DisplayAsProgressLess)
                .ToPropertyOnMainThread(this, x => x.DisplayAsProgressLess);

            var consecutiveProgressNotifications = notifications
                .Where(x => !x.DisplayAsProgressLess && x.ProgressIncrement.HasValue)
                .Select(x => new
                {
                    x.ProgressIncrement.Value,
                    x.Progress,
                    x.ProgressMaxValue
                });

            var speed = consecutiveProgressNotifications
                .Buffer(SpeedDelta)
                .Select(x => x.Sum(y => y.Value) / SpeedDeltaSecs);

            var remaining = consecutiveProgressNotifications
                .Select(x => x.ProgressMaxValue - x.Progress);

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
                    return string.Format(config.SpeedTextTemplate, text, rounded);
                })
                .ToPropertyOnMainThread(this, x => x.Speed);

            consecutiveProgressNotifications
                .Select(x =>
                {
                    return string.Format(config.ProgressTextTemplate,
                        config.Units.GetPresentableText(x.Progress),
                        config.Units.GetPresentableText(x.ProgressMaxValue),
                        x.Progress,
                        x.ProgressMaxValue);
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