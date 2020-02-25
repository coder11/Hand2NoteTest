// ReSharper disable UnassignedGetOnlyAutoProperty

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Hand2Note.ProgressView.Model;
using Hand2Note.ProgressView.Model.DownloadMe;
using Hand2Note.ProgressView.Util;
using Hand2Note.ProgressView.ViewModel.Progress;
using Hand2Note.ProgressView.ViewModel.Progress.Notifications;
using Hand2Note.ProgressView.ViewModel.Progress.Units;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class MainViewModel : ReactiveObject, IActivatableViewModel
    {
        private readonly Style _lightTheme = (Style) Application.Current.Resources["Light"];
        private readonly Style _darkTheme = (Style) Application.Current.Resources["Dark"];

        public MainViewModel()
        {
            Activator = new ViewModelActivator();
            
            LightThemeChecked = true;

            this.WhenAnyValue(x => x.LightThemeChecked)
                .Select(x => x ? _lightTheme : _darkTheme)
                .ToPropertyOnMainThread(this, x => x.Theme);
            
            this.WhenActivated(disposables =>
            {
                InitDownloadMeVm(disposables);
                InitDifferentUnits(disposables);
                InitCustomTexts(disposables);
                InitOperationsChain(disposables);
                InitDisableRestarts(disposables);
                InitDisablePauses(disposables);
                InitDisablePausesForIndividualStage(disposables);
                InitExternallyControllled(disposables);
                InitRealDownload(disposables);

            });
        }
        
        public ViewModelActivator Activator { get; }
        
        public Style? Theme { [ObservableAsProperty] get; }

        [Reactive]
        public bool LightThemeChecked { get; set; }
        
        [Reactive]
        public bool DarkThemeChecked { get; set; }
        
        
        [Reactive]
        public ProgressViewModel? DoDownloadMeVm { get; set; }

        private void InitDownloadMeVm(CompositeDisposable disposables)
        {
            var fsm = DownloadMeFsm.Create()
                .DisposeWith(disposables);
            
            var notifications = DownloadMeProgressViewAdapter.FsmStatesToNotifications(fsm);
            var config = new ProgressViewModelConfig
            {
                ProgressTextTemplate = "Downloaded: {0} / {1}",
                Units = new BytesUnitInfo(),
            };

            DoDownloadMeVm = new ProgressViewModel(notifications, config, fsm.OnStart, fsm.OnStart, fsm.OnPause, fsm.OnResume);
        }
        
        [Reactive]
        public ProgressViewModel? DifferentUnits { get; set; }

        private void InitDifferentUnits(CompositeDisposable disposables)
        {
            var config = new ProgressViewModelConfig
            {
                Units = new FilesUnitInfo(),
                ProgressTextTemplate = "{2}/{1} processed"
            };

            var progress = new DemoProgressOperation(250,
                Enumerable.Range(0, 21)
                    .Select(x => new ProgressNotification(x, 1, 20, $"Extracting HandHistory{x}.zip", true)));

            progress.DisposeWith(disposables);
            
            DifferentUnits = new ProgressViewModel(progress.Notifications, config, progress.Start, progress.Start, progress.Pause, progress.Resume);
        }
        
                
        [Reactive]
        public ProgressViewModel? CustomTexts { get; set; }

        private void InitCustomTexts(CompositeDisposable disposables)
        {
            var config = new ProgressViewModelConfig
            {
                ProgressTextTemplate = "Invaded {2} countries",
                PauseButtonText = "Take a break!",
                RestartButtonText = "Do it again!",
                ResumeButtonText = "Let's go!",
                StartButtonText = "Go on a journey",
                RemainingTimeTextTemplate = "{0:ss} months to go",
                SpeedTextTemplate = "{0} per month"
            };

            var progress = new DemoProgressOperation(
                (new ProgressLessNotification("Packing stuff...", true), 2000),
                (new ProgressNotification(1, 1, 7, "Visiting France!", true), 1000),
                (new ProgressNotification(2, 1, 7, "Visiting Canada!", true), 1200),
                (new ProgressNotification(3, 1, 7, "Visiting Uk!", true), 1300),
                (new ProgressNotification(4, 1, 7, "Visiting Russia!", true), 1600),
                (new ProgressNotification(5, 1, 7, "Visiting Shire!", true), 900),
                (new ProgressNotification(6, 1, 7, "Visiting Northrend!", true), 1000),
                (new ProgressNotification(7, 1, 7, "Visiting Arstotzka!", true), 1100),
                (new ProgressLessNotification("Home, sweet home!", true), 2000));

            progress.DisposeWith(disposables);
            
            progress.PausedCaption = "Relaxing...";
            progress.FinishedCaption = "Huh! We made it!";
            
            CustomTexts = new ProgressViewModel(progress.Notifications, config, progress.Start, progress.Start, progress.Pause, progress.Resume);
        }
        
        
                
        [Reactive]
        public ProgressViewModel? OperationsChain { get; set; }

        private void InitOperationsChain(CompositeDisposable disposables)
        {
            var progress = new DemoProgressOperation(GetOperationChainData().ToArray());
            progress.DisposeWith(disposables);
            OperationsChain = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start, pause: progress.Pause, resume:progress.Resume);
        }

        private IEnumerable<(IProgressNotification Notification, int TimeoutMs)> GetOperationChainData()
        {
            yield return (new ProgressLessNotification("Connecting", true), 2000);

            for(int i = 0; i < 21; i++)
            {
                yield return (new ProgressNotification(i, 1, 20, "Downloading", true), 100);
            }
            
            yield return (new ProgressLessNotification("Verifying", true), 2000);
            
            for(int i = 0; i < 21; i++)
            {
                yield return (new ProgressNotification(i, 1, 20, "Extracting", true), 100);
            }
            
            yield return (new ProgressLessNotification("Installing", true), 2000);
        }


        [Reactive]
        public ProgressViewModel? DisableRestarts { get; set; }

        private void InitDisableRestarts(CompositeDisposable disposables)
        {
            var progress = NewSimpleProgress();
            progress.DisposeWith(disposables);
            DisableRestarts = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start, pause: progress.Pause, resume:progress.Resume);
        }

        
        [Reactive]
        public ProgressViewModel? DisablePauses { get; set; }

        private void InitDisablePauses(CompositeDisposable disposables)
        {
            var progress = NewSimpleProgress();
            progress.DisposeWith(disposables);
            DisablePauses = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start);
        }
        
        
        [Reactive]
        public ProgressViewModel? DisablePausesForIndividualStage { get; set; }
        
        private void InitDisablePausesForIndividualStage(CompositeDisposable disposables)
        {
            IEnumerable<IProgressNotification> pausable = Enumerable.Range(0, 5)
                .Select(x => new ProgressNotification(x, 1, 4, $"This can be paused", true));
            
            var pausableProgressLess = new ProgressLessNotification("ProgressLess that can be paused", true);
            
            var unPausable = Enumerable.Range(0, 5)
                .Select(x => new ProgressNotification(x, 1, 4, $"This cannot be paused", false));
            
            var unPausableProgressLess = new ProgressLessNotification("ProgressLess that cannot be paused", false);

            var all = new[] {pausableProgressLess}
                .Concat(pausable)
                .Concat(new[] {unPausableProgressLess})
                .Concat(unPausable)
                .ToArray(); 
            
            var progress = new DemoProgressOperation(2000, all);
            progress.DisposeWith(disposables);
            
            DisablePausesForIndividualStage = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start, pause: progress.Pause, resume: progress.Resume);
        }
        
        
        [Reactive]
        public ReactiveCommand<Unit, Unit>? StartTwoViews { get; set; }
        [Reactive]
        public ProgressViewModel? Follower1 { get; set; }
        [Reactive]
        public ProgressViewModel? Follower2 { get; set; }
        
        private void InitExternallyControllled(CompositeDisposable disposables)
        {
            var progress = NewSimpleProgress()
                .DisposeWith(disposables);

            var canExecute = this.WhenAnyObservable(x => x.StartTwoViews)
                .Select(x => false)
                .StartWith(true);

            StartTwoViews = ReactiveCommand.Create(progress.Start, canExecute);
            
            var vm = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig());
            Follower1 = vm;
            Follower2 = vm;
        }
        
        
        private DemoProgressOperation NewSimpleProgress()
        {
            return new DemoProgressOperation(250,
                Enumerable.Range(0, 21)
                    .Select(x => new ProgressNotification(x, 1, 20, $"Doing work", true)));
        }
        
        
        [Reactive]
        public ProgressViewModel? RealDownload { get; set; }

        private const string url = "http://h2n-uptoyou.azureedge.net/main/Hand2NoteInstaller.exe";
        
        private void InitRealDownload(CompositeDisposable disposables)
        {
            var client = new WebClient();
            client.DisposeWith(disposables);
            
            Action run = () => client.DownloadFileTaskAsync(url, GetTempFile());

            var dlProgress = Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
                x => client.DownloadProgressChanged += x,
                x => client.DownloadProgressChanged -= x,
                RxApp.TaskpoolScheduler);

            var finished = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>(
                x => client.DownloadFileCompleted += x,
                x => client.DownloadFileCompleted -= x,
                RxApp.TaskpoolScheduler);

            var notifications = dlProgress
                .Select(x => new
                {
                    Progress = (int) x.EventArgs.BytesReceived,
                    ProgressMaxValue = (int) x.EventArgs.TotalBytesToReceive,
                    Increment = 0,
                })
                .Scan(new {Progress = 0, ProgressMaxValue = 1, Increment = 0},
                    (acc, cur) => new
                    {
                        cur.Progress,
                        cur.ProgressMaxValue, 
                        Increment = cur.Progress - acc.Progress
                    })
                .Select(x => new ProgressNotification(x.Progress, x.Increment, x.ProgressMaxValue, "Downloading",false))
                .Merge<IProgressNotification>(finished.Select(x => new FinishedNotification("Finished")));
            
            RealDownload = new ProgressViewModel(notifications, new ProgressViewModelConfig { Units = new BytesUnitInfo() }, run);
        }
        
        private string GetTempFile()
        {
            return System.IO.Path.GetTempPath() + Guid.NewGuid();
        }
    }
}