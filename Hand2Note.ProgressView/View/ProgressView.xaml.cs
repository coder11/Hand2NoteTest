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
                    vm => vm.Progress,
                    v => v.Progress.Value
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressMaxValue,
                    v => v.Progress.Maximum
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.DisplayAsProgressLess,
                    v => v.Progress.IsIndeterminate
                ));

                disposable(this.BindCommand(ViewModel,
                    vm => vm.Command,
                    v => v.ControlButton
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.CommandButtonText,
                    v => v.ControlButton.Content,
                    x => x?.ToUpperInvariant()));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressText,
                    v => v.ProgressText.Text
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.ProgressTextVisible,
                    v => v.ProgressText.Visibility,
                    vmToViewConverterOverride: new BooleanToVisibilityTypeConverter()
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.RemainingTime,
                    v => v.RemainingTime.Text
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.RemainingTimeVisible,
                    v => v.RemainingTime.Visibility,
                    vmToViewConverterOverride: new BooleanToVisibilityTypeConverter()
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.Speed,
                    v => v.Speed.Text
                ));

                disposable(this.OneWayBind(ViewModel,
                    vm => vm.SpeedVisible,
                    v => v.Speed.Visibility,
                    vmToViewConverterOverride: new BooleanToVisibilityTypeConverter()
                ));
            });
        }
    }
}