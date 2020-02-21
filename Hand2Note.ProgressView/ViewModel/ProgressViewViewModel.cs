using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hand2Note.ProgressView.Model;
using Hand2Note.ProgressView.Model.Operations;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class ProgressViewModel : ReactiveObject
    {
        private readonly IOperationWithProgressService _service;

        private object _operationState;

        const double SpeedDeltaSecs = 1;
        private readonly TimeSpan SpeedDelta = TimeSpan.FromSeconds(SpeedDeltaSecs);

        private readonly TimeSpan TextRefreshRate = TimeSpan.FromSeconds(1);
        
        public ProgressViewModel(IOperationWithProgressService service)
        {
            _service = service;

            var (state, stages) = service.Init();
            _operationState = state;
            
            _startCommand = ReactiveCommand.Create(() =>
            {
                _operationState = _service.Start(_operationState);
            });

            _pauseCommand = ReactiveCommand.Create(() =>
            {
                _operationState = _service.Pause(_operationState);
            });
            
            _resumeCommand = ReactiveCommand.Create(() =>
            {
                _operationState = _service.Resume(_operationState);
            });

            stages.Select(x => x.Caption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Caption);

            stages.Select(x => x.IsProgressless)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.IsProgresslessOperation);

            var progressNotifications = stages
                .Where(x => !x.IsProgressless && !x.HasFinished)
                .Select(op =>
                {
                    var progress = op.Progress
                        .Scan((acc, cur) => acc + cur);
                        // .StartWith(0);

                    var speed = op.Progress
                        .Buffer(SpeedDelta)
                        .Select(x => x.Sum() / SpeedDeltaSecs);

                    var remaining = progress
                        .Select(x => op.ProgressMax - x);

                    var remainingTime = Observable.CombineLatest(
                        remaining,
                        speed,
                        (r, s) => r / s);

                    return new
                    {
                        Operation = op,
                        Progress = progress,
                        
                        ProgressText = progress.Select(x =>
                            string.Format("{0} / {1}",
                            op.UnitInfo.GetPresentableText(x),
                            op.UnitInfo.GetPresentableText(op.ProgressMax.Value))),
                        
                        ProgressMax = op.ProgressMax,
                        
                        Speed = speed
                            .Select(x =>
                            {
                                if (Double.IsInfinity(x) || Double.IsNaN(x))
                                {
                                    return "?";
                                }

                                var rounded = Math.Round(x);
                                var text = op.UnitInfo.GetPresentableText((int) rounded);
                                return $" {text}/s";
                            }),
                        
                        Remaining = remaining
                            .Where(x => x.HasValue)
                            .Select(x => op.UnitInfo.GetPresentableText(x.Value)),
                        
                        RemainingTime = remainingTime
                            .Where(x => x.HasValue)
                            .Select(x => x.Value)
                            .Select(x =>
                            {
                                if (Double.IsInfinity(x) || Double.IsNaN(x))
                                {
                                    return "?";
                                }
                                
                                var rounded = Math.Round(x);
                                var ts = TimeSpan.FromSeconds(rounded);
                                return string.Format("{0:mm\\:ss}", ts);
                            }),
                        
                        Increments = progress.Select(x => op.UnitInfo.GetPresentableText(x)),
                    };
                });
            
            progressNotifications
                .SelectMany(x => x.Speed)
                .Select(x => $"Speed: {x}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Speed);
            
            progressNotifications
                .SelectMany(x => x.ProgressText)
                .Sample(TextRefreshRate)
                .Select(x => $"Progress: {x}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Done);
            
            progressNotifications
                .SelectMany(x => x.Progress)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ProgressValue);
            
            progressNotifications
                .Select(x => x.ProgressMax ?? 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ProgressMax);

            progressNotifications
                .SelectMany(x => x.RemainingTime)
                .Sample(TextRefreshRate)
                .Select(x => $"Time remaining: {x}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.RemainingTime);
            
            stages.Select(x => x.Caption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Caption);

            var finishedStage = stages.Where(x => x.HasFinished); 
            
            var isRunning = _startCommand
                .Select(x => true)
                .Merge(finishedStage.Select(x => false));

            var neverRun = _startCommand
                .Select(x => false)
                .StartWith(true);
            
            var hasFinished = isRunning
                .Select(x => !x);

            var canPauseCurrentStage = stages
                .Select(x => x.CanPause);

            var isPaused = Observable.Merge(
                _pauseCommand.Select(x => true),
                _resumeCommand.Select(x => false))
                .StartWith(false);

            Observable.CombineLatest(
                    isRunning, 
                    neverRun,
                    canPauseCurrentStage,
                    isPaused,
                    (isRunning_, neverRun_, canPauseCurrentStage_, isPaused_) =>
                    {
                        if (neverRun_)
                            return _startCommand;

                        if (isRunning_ && canPauseCurrentStage_)
                        {
                            return isPaused_ ? _resumeCommand : _pauseCommand;
                        }

                        return _startCommand;
                    })
                .StartWith(_startCommand)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.Command);

            
            var buttonCaption = Observable.Merge(
                Observable.Return("Start"),
                stages.Where(x => x.CanPause)
                    .CombineLatest(isPaused, (_, p) => p)
                    .Select(paused => paused ? "Resume" : "Pause"),
                stages.Where(x => x.HasFinished)
                    .Select(x => "Restart"));
                
            buttonCaption
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.ButtonText);
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