using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Remoting.Messaging;
using Hand2Note.ProgressView.Model.DownloadMe;
using Hand2Note.ProgressView.Model.Operations;
using Hand2Note.ProgressView.Util;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class ProgressViewModel : ReactiveObject
    {
        const double SpeedDeltaSecs = 1;
        private readonly TimeSpan SpeedDelta = TimeSpan.FromSeconds(SpeedDeltaSecs);

        private readonly TimeSpan TextRefreshRate = TimeSpan.FromSeconds(1);
        
        public ProgressViewModel()
        {
            var fsm = DownloadMeFsm.Create();

            var stateInfos = fsm.States
                .Select(x =>
                {
                    var isProgressless = x.State.In(
                        DownloadMeStateType.Connecting,
                        DownloadMeStateType.Finishing,
                        DownloadMeStateType.Pausing,
                        DownloadMeStateType.Starting);

                    string caption = string.Empty;
                    switch (x.State)
                    {
                        case DownloadMeStateType.Starting:
                        case DownloadMeStateType.Connecting:
                            caption = "Connecting";
                            break;
                        
                        case DownloadMeStateType.Downloading:
                            caption = "Downloading";
                            break;
                            
                        case DownloadMeStateType.Finishing:
                            caption = "Finishing";
                            break;
                        
                        case DownloadMeStateType.Paused:
                            caption = "Paused";
                            break;
                            
                        case DownloadMeStateType.Finished:
                            caption = "Finished";
                            break;
                    }

                    bool allowPause = x.State.In(
                        DownloadMeStateType.Connecting,
                        DownloadMeStateType.Downloading,
                        DownloadMeStateType.Finishing);

                    bool HasFinished = x.State == DownloadMeStateType.Finished;
                    
                    bool allowResume = x.State == DownloadMeStateType.Paused;
                    
                    return new
                    {
                        x.Progress,
                        x.ProgressIncrement,
                        ProgressMaxValue = fsm.TotalBytesToDownload,
                        
                        HasFinished = HasFinished, 
                        
                        AllowPause = allowPause,
                        AllowResume = allowResume,
                        
                        HideSpeed = false,
                        HideProgressText = false,
                        HideRemainingTime = false,
                        
                        DisplayAsProgressless = isProgressless,
                        Caption = caption,
                    };
                })
                .StartWith(new
                {
                    Progress = 0,
                    ProgressIncrement = (int?) null,
                    ProgressMaxValue = 1,
                    HasFinished = false,
                    AllowPause = false,
                    AllowResume = false,
                    HideSpeed = true,
                    HideProgressText = true,
                    HideRemainingTime = true,
                    DisplayAsProgressless = false,
                    Caption = string.Empty
                });

            var startCommandFired = this.WhenAnyObservable(x => x._startCommand); 

            var isRunning = startCommandFired
                .Select(x => true)
                .Merge(stateInfos
                    .Where(x => x.HasFinished)
                    .Select(x => false))
                .StartWith(false);
            
            var wasNeverRun = startCommandFired
                .Select(x => true)
                .StartWith(true);
                
            _startCommand = ReactiveCommand.Create(fsm.OnStart, isRunning.Select(x => !x));
            
            _restartCommand = _startCommand;
            
            _pauseCommand = ReactiveCommand.Create(fsm.OnPause, stateInfos.Select(x => x.AllowPause)
                .ObserveOn(RxApp.MainThreadScheduler));
            
            _resumeCommand = ReactiveCommand.Create(fsm.OnResume, stateInfos.Select(x => x.AllowResume)
                .ObserveOn(RxApp.MainThreadScheduler));

            var currentlyAvailableCommand =
                Observable.CombineLatest(stateInfos, isRunning, wasNeverRun,
                    (stateInfo, isRunning_, wasNeverRun_) =>
                    {
                        if (isRunning_ && stateInfo.AllowPause)
                            return new
                            {
                                Command = _pauseCommand,
                                Caption = "Pause"
                            };

                        if (isRunning_ && stateInfo.AllowResume)
                            return new
                            {
                                Command = _resumeCommand,
                                Caption = "Resume"
                            };

                        if (wasNeverRun_)
                            return new
                            {
                                Command = _startCommand,
                                Caption = "Start"
                            };

                        return new
                        {
                            Command = _startCommand,
                            Caption = "Restart"
                        };
                    });
                
            currentlyAvailableCommand
                .Select(x => x.Command)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Command);

            currentlyAvailableCommand
                .Select(x => x.Caption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.CommandButtonText);

            stateInfos.Select(x => x.Caption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Caption);

            stateInfos.Select(x => x.Progress)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Progress);
            
            stateInfos.Select(x => x.ProgressMaxValue)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ProgressMaxValue);
            
            stateInfos.Select(x => x.DisplayAsProgressless)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.DisplayAsProgressless);

            var consecutiveProgressNotifications = stateInfos
                .Select(x => new
                {
                    IsProgressless = x.DisplayAsProgressless,
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
                        return "?";
                    }

                    var rounded = (int) Math.Round(x);
                    var text = new BytesUnitInfo().GetPresentableText(rounded);
                    return $"Speed: {text}/s";
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Speed);
            
            consecutiveProgressNotifications
                .Select(x =>
                {
                    return string.Format("Downloaded: {0} / {1}",
                        new BytesUnitInfo().GetPresentableText(x.TotalProgress),
                        new BytesUnitInfo().GetPresentableText(x.TotalBytesToDownload));
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ProgressText);
                
            remainingTime
                .Select(x =>
                {
                    if (Double.IsInfinity(x) || Double.IsNaN(x))
                    {
                        return "?";
                    }
                                
                    var rounded = Math.Round(x);
                    var ts = TimeSpan.FromSeconds(rounded);
                    return string.Format("Time remaining: {0:mm\\:ss}", ts);
                })
                .Sample(TextRefreshRate)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.RemainingTime);
        }
        
        private ReactiveCommand<Unit, Unit> _startCommand;
        private ReactiveCommand<Unit, Unit> _restartCommand;
        private ReactiveCommand<Unit, Unit> _pauseCommand;
        private ReactiveCommand<Unit, Unit> _resumeCommand;

        public int ProgressMaxValue { [ObservableAsProperty] get; }

        public int Progress { [ObservableAsProperty] get; }
        
        public bool DisplayAsProgressless { [ObservableAsProperty] get; }

        public string Speed { [ObservableAsProperty] get; }
        
        public string SpeedVisible { [ObservableAsProperty] get; }
        
        public string ProgressText { [ObservableAsProperty] get; }
        
        public string ProgressTextVisible { [ObservableAsProperty] get; }
        
        public string RemainingTime { [ObservableAsProperty] get; }
        
        public string RemainingTimeVisible { [ObservableAsProperty] get; }
        
        public string CommandButtonText { [ObservableAsProperty] get; }

        public string Caption { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> Command { [ObservableAsProperty] get; }
    }
}