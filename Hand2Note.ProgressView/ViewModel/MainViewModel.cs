// ReSharper disable UnassignedGetOnlyAutoProperty

using Hand2Note.ProgressView.Model.DownloadMe;
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

            DoDownloadMeVm = new ProgressViewModel(notifications, fsm.OnStart, fsm.OnPause, fsm.OnResume);
        }

        [Reactive]
        public ProgressViewModel DoDownloadMeVm { get; set; }
    }
}