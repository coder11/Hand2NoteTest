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
                    vm => vm.ButtonText,
                    v => v.ControlButton.Content
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressMax,
                    v => v.Progress.Maximum
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressValue,
                    v => v.Progress.Value
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.IsProgresslessOperation,
                    v => v.Progress.IsIndeterminate
                ));
                
                disposable(this.BindCommand(ViewModel,
                    vm => vm.Command,
                    v => v.ControlButton
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Done,
                    v => v.DonePart.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Done,
                    v => v.DonePart.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.RemainingTime,
                    v => v.RemainginTimePart.Text
                ));
                
                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Speed,
                    v => v.SpeedPart.Text
                ));
            });
        }
    }
}