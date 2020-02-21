using Hand2Note.ProgressView.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            FileDownload = new ProgressViewModel(new DownloadMeService());
        }

        [Reactive]
        public ProgressViewModel FileDownload { get; set; }
    }
}