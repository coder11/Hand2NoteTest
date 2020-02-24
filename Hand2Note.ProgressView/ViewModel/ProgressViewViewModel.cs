using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hand2Note.ProgressView.Model;
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
                        
                        case DownloadMeStateType.Pausing:
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
                    
                    bool allowStart = x.State.In(
                        DownloadMeStateType.Initial,
                        DownloadMeStateType.Finished);

                    bool allowResume = x.State == DownloadMeStateType.Paused;
                    
                    return new
                    {
                        x.TotalProgress,
                        x.ProgressIncrement,
                        fsm.TotalBytesToDownload,
                        
                        AllowPause = allowPause,
                        AllowStart = allowStart,
                        AllowResume = allowResume,
                        
                        
                        IsProgressless = isProgressless,
                        Caption = caption,
                    };
                });
            
            _startCommand = ReactiveCommand.Create(fsm.OnStart, stateInfos
                .Select(x => x.AllowStart)
                .StartWith(true)
                .ObserveOn(RxApp.MainThreadScheduler));
            
            _pauseCommand = ReactiveCommand.Create(fsm.OnPause, stateInfos.Select(x => x.AllowPause)
                .ObserveOn(RxApp.MainThreadScheduler));
            
            _resumeCommand = ReactiveCommand.Create(fsm.OnResume, stateInfos.Select(x => x.AllowResume)
                .ObserveOn(RxApp.MainThreadScheduler));

            stateInfos.Select(x =>
                {
                    if (x.AllowStart)
                        return _startCommand;

                    if (x.AllowPause)
                        return _pauseCommand;

                    if (x.AllowResume)
                        return _resumeCommand;

                    return _startCommand;
                })
                .StartWith(_startCommand)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Command);

            stateInfos.Select(x =>
                {
                    if (x.AllowStart)
                        return "Start";

                    if (x.AllowPause)
                        return "Pause";

                    if (x.AllowResume)
                        return "Resume";

                    return "Start";
                })
                .StartWith("Start")
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ButtonText);
            
            stateInfos.Select(x => x.Caption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Caption);

            stateInfos.Select(x => x.TotalProgress)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ProgressValue);
            
            stateInfos.Select(x => x.TotalBytesToDownload)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ProgressMax);
            
            stateInfos.Select(x => x.IsProgressless)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.IsProgresslessOperation);

            var consecutiveProgressNotifications = stateInfos
                .Select(x => new
                {
                    x.IsProgressless,
                    x.ProgressIncrement,
                    x.TotalProgress,
                    x.TotalBytesToDownload
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
                .ToPropertyEx(this, x => x.Done);
                
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
        private ReactiveCommand<Unit, Unit> _pauseCommand;
        private ReactiveCommand<Unit, Unit> _resumeCommand;

        public int ProgressMax { [ObservableAsProperty] get; }

        public int ProgressValue { [ObservableAsProperty] get; }
        
        public bool IsProgresslessOperation { [ObservableAsProperty] get; }

        public string Speed { [ObservableAsProperty] get; }
        
        public string Done { [ObservableAsProperty] get; }
        
        public string RemainingTime { [ObservableAsProperty] get; }
        
        public string ButtonText { [ObservableAsProperty] get; }

        public string Caption { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> Command { [ObservableAsProperty] get; }
    }
}