// ReSharper disable UnassignedGetOnlyAutoProperty

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Hand2Note.ProgressView.Util;
using Hand2Note.ProgressView.ViewModel.Progress;
using Hand2Note.ProgressView.ViewModel.Progress.Notifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class ProgressViewModel : ReactiveObject, IActivatableViewModel
    {
        private const double SpeedDeltaSecs = 1;
        private TimeSpan SpeedDelta => TimeSpan.FromSeconds(SpeedDeltaSecs);

        private readonly TimeSpan TextRefreshRate = TimeSpan.FromSeconds(1);
        
        private ReactiveCommand<Unit, Unit>? _startCommand;
        private ReactiveCommand<Unit, Unit>? _restartCommand;
        private ReactiveCommand<Unit, Unit>? _pauseCommand;
        private ReactiveCommand<Unit, Unit>? _resumeCommand;
        
        public ProgressViewModel(IObservable<IProgressNotification> notifications, 
            ProgressViewModelConfig config, 
            Action? start = null, 
            Action? restart = null,
            Action? pause = null, 
            Action? resume = null)
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                InitializeObservables(disposables, notifications, config, start, restart, pause, resume);
            });
        }

        public ViewModelActivator Activator { get; }

        public int ProgressMaxValue { [ObservableAsProperty] get; }

        public int Progress { [ObservableAsProperty] get; }
        
        public bool DisplayAsProgressLess { [ObservableAsProperty] get; }

        public string? Speed { [ObservableAsProperty] get; }
        
        public bool SpeedVisible { [ObservableAsProperty] get; }
        
        public string? ProgressText { [ObservableAsProperty] get; }
        
        public bool ProgressTextVisible { [ObservableAsProperty] get; }
        
        public string? RemainingTime { [ObservableAsProperty] get; }
        
        public bool RemainingTimeVisible { [ObservableAsProperty] get; }
        
        public string? CommandButtonText { [ObservableAsProperty] get; }

        public string? Caption { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit>? Command { [ObservableAsProperty] get; }
        
        private void InitializeObservables(CompositeDisposable disposables, 
            IObservable<IProgressNotification> notifications,
            ProgressViewModelConfig config,
            Action? start, 
            Action? restart,
            Action? pause,
            Action? resume)
        {
            var disableStart = start == null;
            var disableRestart = restart == null;
            var disablePause = pause == null || resume == null;

            start ??= ActionHelpers.Empty;
            restart ??= ActionHelpers.Empty;
            pause ??= ActionHelpers.Empty;
            resume ??= ActionHelpers.Empty;

            var canResume = notifications
                .Select(x => x is PausedNotification)
                .StartWith(false)
                .DistinctUntilChanged();
            
            var allowPause = notifications.Select(x => x.AllowPause)
                .StartWith(false)
                .DistinctUntilChanged();
            
            var hasFinished = notifications
                .Select(x => x is FinishedNotification)
                .StartWith(false);
            
            var isPaused = notifications
                .Select(x => x is PausedNotification)
                .StartWith(false);

            var canStartExecute = new BehaviorSubject<bool>(!disableStart);
            var canRestartExecute = new BehaviorSubject<bool>(!disableRestart);
            var canPauseExecute = new BehaviorSubject<bool>(!disablePause);
            var canResumeExecute = new BehaviorSubject<bool>(!disablePause);
            
            _startCommand = ReactiveCommand.Create(start, canStartExecute);
            _restartCommand = ReactiveCommand.Create(restart, canRestartExecute);
            _pauseCommand = ReactiveCommand.Create(pause, canPauseExecute);
            _resumeCommand = ReactiveCommand.Create(resume, canResumeExecute);
            
            var pauseCommandCooldown = _pauseCommand.TrueUntil(isPaused);

            allowPause
                .BooleanAnd(pauseCommandCooldown.Negate())
                .BooleanAnd(!disablePause)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(canPauseExecute)
                .DisposeWith(disposables);
            
            _startCommand
                .Select(x => false)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(canStartExecute)
                .DisposeWith(disposables);

            canResume
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(canResumeExecute)
                .DisposeWith(disposables);

            hasFinished
                .BooleanAnd(!disableRestart)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(canRestartExecute)
                .DisposeWith(disposables);
            
            var wasNeverRun = _startCommand.TrueBefore();
            var isRunning = _startCommand.Merge(_restartCommand)
                .TrueUntil(notifications.OfType<FinishedNotification>())
                .StartWith(false);

            var currentlyAvailableCommand =
                Observable.CombineLatest(allowPause, isRunning, wasNeverRun, canResume,
                    (allowPause_, isRunning_, wasNeverRun_, canResume_) =>
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

                        if (allowPause_)
                            return new
                            {
                                Command = _pauseCommand,
                                Caption = config.PauseButtonText
                            };
                        
                        if (canResume_)
                            return new
                            {
                                Command = _resumeCommand,
                                Caption = config.ResumeButtonText
                            };

                        return null;
                    });

            currentlyAvailableCommand
                .Where(x => x != null)
                .Select(x => x!.Command)
                .ToPropertyOnMainThread(this, x => x.Command);

            currentlyAvailableCommand
                .Where(x => x != null)
                .Select(x => x!.Caption)
                .ToPropertyOnMainThread(this, x => x.CommandButtonText);

            notifications.Select(x => x.Caption)
                .ToPropertyOnMainThread(this, x => x.Caption);

            var progressNotifications = notifications.OfType<ProgressNotification>();
            
            progressNotifications.Select(x => x.Progress)
                .ToPropertyOnMainThread(this, x => x.Progress);

            var progressValueChangeNotifications = Observable.Merge(progressNotifications
                    .Select(x => new
                    {
                        x.Progress,
                        x.ProgressMaxValue
                    }),
                notifications.OfType<ProgressInitNotification>()
                    .Select(x => new
                    {
                        x.Progress,
                        x.ProgressMaxValue
                    }));
            ;
            
            progressValueChangeNotifications
                .Select(x => x.ProgressMaxValue)
                .ToPropertyOnMainThread(this, x => x.ProgressMaxValue, 1);

            notifications
                .Select(x => x is ProgressLessNotification)
                .ToPropertyOnMainThread(this, x => x.DisplayAsProgressLess);

            var speed = progressNotifications
                .Buffer(SpeedDelta)
                .Select(x => x.Sum(y => y.ProgressIncrement) / SpeedDeltaSecs);

            var remaining = progressNotifications
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

            progressValueChangeNotifications
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

            var showSpeed = notifications
                .Select(x => x is ProgressNotification);
            
            showSpeed
                .ToPropertyOnMainThread(this, x => x.SpeedVisible);
            
            showSpeed
                .ToPropertyOnMainThread(this, x => x.RemainingTimeVisible);

            progressValueChangeNotifications.TrueAfter()
                .ToPropertyOnMainThread(this, x => x.ProgressTextVisible);
        }
    }
}