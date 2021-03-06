namespace Hand2Note.ProgressView.ViewModel.Progress.Notifications
{
    public class ProgressInitNotification : IProgressNotification
    {
        public ProgressInitNotification(int progress, int progressMaxValue, string caption = "")
        {
            Progress = progress;
            Caption = caption;
            ProgressMaxValue = progressMaxValue;
        }

        public int ProgressMaxValue { get; }
        public int Progress { get; }

        public string Caption { get; }
        public bool AllowPause => false;
    }
}