using Hand2Note.ProgressView.ViewModel;
using ReactiveUI;

namespace Hand2Note.ProgressView.View
{
    public partial class ProgressView
    {
        public ProgressView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Caption,
                    v => v.Caption.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.CommandButtonText,
                    v => v.ControlButton.Content
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressMaxValue,
                    v => v.Progress.Maximum
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Progress,
                    v => v.Progress.Value
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.DisplayAsProgressless,
                    v => v.Progress.IsIndeterminate
                ));
                
                disposable(this.BindCommand(ViewModel,
                    vm => vm.Command,
                    v => v.ControlButton
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressText,
                    v => v.ProgressText.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressText,
                    v => v.ProgressText.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.RemainingTime,
                    v => v.RemainingTime.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Speed,
                    v => v.Speed.Text
                ));
            });
        }
    }
}