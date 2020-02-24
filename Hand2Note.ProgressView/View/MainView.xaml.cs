using ReactiveUI;

namespace Hand2Note.ProgressView.View
{
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                disposable(this.Bind(ViewModel,
                    vm => vm.DoDownloadMeVm,
                    v => v.DownloadMe.ViewModel
                ));
            });
        }
    }
}