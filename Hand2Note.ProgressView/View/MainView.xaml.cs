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
                
                disposable(this.Bind(ViewModel,
                    vm => vm.LightThemeChecked,
                    v => v.LightThemeRadioBtn.IsChecked
                ));
                
                disposable(this.Bind(ViewModel,
                    vm => vm.DarkThemeChecked,
                    v => v.DarkThemeRadioBtn.IsChecked
                ));
                
                disposable(this.Bind(ViewModel,
                    vm => vm.Theme,
                    v => v.Style
                ));
            });
        }
    }
}