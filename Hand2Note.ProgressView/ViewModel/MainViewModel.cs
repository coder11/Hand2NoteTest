// ReSharper disable UnassignedGetOnlyAutoProperty

using Hand2Note.ProgressView.Model.DownloadMe;
using Hand2Note.ProgressView.ViewModel.Progress;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            var fsm = DownloadMeFsm.Create();
            var notifications = DownloadMeProgressViewAdapter.FsmStatesToNotifications(fsm);
            var config = new ProgressViewModelConfig()
            {
                Units = new BytesUnitInfo()
            };
            
            DoDownloadMeVm = new ProgressViewModel(notifications, config, fsm.OnStart, fsm.OnPause, fsm.OnResume);
        }

        [Reactive]
        public ProgressViewModel DoDownloadMeVm { get; set; }
    }
}