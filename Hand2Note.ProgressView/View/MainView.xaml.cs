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

                disposable(this.Bind(ViewModel,
                    vm => vm.DoDownloadMeVm,
                    v => v.DownloadMe.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.DifferentUnits,
                    v => v.DifferentUnits.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.CustomTexts,
                    v => v.CustomTexts.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.OperationsChain,
                    v => v.OperationsChain.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.DisableRestarts,
                    v => v.DisableRestarts.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.DisablePauses,
                    v => v.DisablePauses.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.DisablePausesForIndividualStage,
                    v => v.DisablePausesForIndividualStage.ViewModel
                ));

                disposable(this.BindCommand(ViewModel,
                    vm => vm.StartTwoViews,
                    v => v.StartTwoViews
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.Follower1,
                    v => v.Follower1.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.Follower2,
                    v => v.Follower2.ViewModel
                ));

                disposable(this.Bind(ViewModel,
                    vm => vm.RealDownload,
                    v => v.RealDownload.ViewModel
                ));
            });
        }
    }
}