// ReSharper disable UnassignedGetOnlyAutoProperty

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
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
    public class MainViewModel : ReactiveObject
    {
        private readonly Style _lightTheme = (Style) Application.Current.Resources["Light"];
        private readonly Style _darkTheme = (Style) Application.Current.Resources["Dark"];
        
        public MainViewModel()
        {
            InitDownloadMeVm();
            InitDifferentUnits();
            InitCustomTexts();
            InitDisableRestarts();
            InitDisablePauses();
            InitDisablePausesForIndividualStage();
            InitExternallyControllled();
        
            LightThemeChecked = true;
            
            this.WhenAnyValue(x => x.LightThemeChecked)
                .Select(x => x ? _lightTheme : _darkTheme)
                .ToPropertyOnMainThread(this, x => x.Theme);
        }
        
        public Style Theme { [ObservableAsProperty] get; }

        [Reactive]
        public bool LightThemeChecked { get; set; }
        
        [Reactive]
        public bool DarkThemeChecked { get; set; }
        
        
        [Reactive]
        public ProgressViewModel DoDownloadMeVm { get; set; }

        private void InitDownloadMeVm()
        {
            var fsm = DownloadMeFsm.Create();
            var notifications = DownloadMeProgressViewAdapter.FsmStatesToNotifications(fsm);
            var config = new ProgressViewModelConfig
            {
                ProgressTextTemplate = "Downloaded: {0} / {1}",
                Units = new BytesUnitInfo(),
            };

            DoDownloadMeVm = new ProgressViewModel(notifications, config, fsm.OnStart, fsm.OnStart, fsm.OnPause, fsm.OnResume);
        }
        
        [Reactive]
        public ProgressViewModel DifferentUnits { get; set; }

        private void InitDifferentUnits()
        {
            var config = new ProgressViewModelConfig
            {
                Units = new FilesUnitInfo(),
                ProgressTextTemplate = "{2}/{1} processed"
            };

            var progress = new DemoProgressOperation(250,
                Enumerable.Range(0, 21)
                    .Select(x => new ProgressNotification(x, 1, 20, $"Extracting HandHistory{x}.zip", true)));
            
            DifferentUnits = new ProgressViewModel(progress.Notifications, config, progress.Start, progress.Start, progress.Pause, progress.Resume);
        }
        
                
        [Reactive]
        public ProgressViewModel CustomTexts { get; set; }

        private void InitCustomTexts()
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

            progress.PausedCaption = "Relaxing...";
            progress.FinishedCaption = "Huh! We made it!";
            
            CustomTexts = new ProgressViewModel(progress.Notifications, config, progress.Start, progress.Start, progress.Pause, progress.Resume);
        }
        
        
        [Reactive]
        public ProgressViewModel DisableRestarts { get; set; }

        private void InitDisableRestarts()
        {
            var progress = NewSimpleProgress();
            DisableRestarts = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start, pause: progress.Pause, resume:progress.Resume);
        }

        
        [Reactive]
        public ProgressViewModel DisablePauses { get; set; }

        private void InitDisablePauses()
        {
            var progress = NewSimpleProgress();
            DisablePauses = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start);
        }
        
        
        [Reactive]
        public ProgressViewModel DisablePausesForIndividualStage { get; set; }
        
        private void InitDisablePausesForIndividualStage()
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
            
            DisablePausesForIndividualStage = new ProgressViewModel(progress.Notifications, new ProgressViewModelConfig(), start: progress.Start, pause: progress.Pause, resume: progress.Resume);
        }
        
        
        [Reactive]
        public ReactiveCommand<Unit, Unit> StartTwoViews { get; set; }
        [Reactive]
        public ProgressViewModel Follower1 { get; set; }
        [Reactive]
        public ProgressViewModel Follower2 { get; set; }
        
        private void InitExternallyControllled()
        {
            var progress = NewSimpleProgress();

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
    }
}