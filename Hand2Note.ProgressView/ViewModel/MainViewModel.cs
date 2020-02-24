// ReSharper disable UnassignedGetOnlyAutoProperty

using System.Reactive.Linq;
using System.Windows;
using Hand2Note.ProgressView.Model.DownloadMe;
using Hand2Note.ProgressView.Util;
using Hand2Note.ProgressView.ViewModel.Progress;
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
                Units = new BytesUnitInfo(),
            };

            DoDownloadMeVm = new ProgressViewModel(notifications, config, fsm.OnStart, fsm.OnPause, fsm.OnResume);
        }
    }
}