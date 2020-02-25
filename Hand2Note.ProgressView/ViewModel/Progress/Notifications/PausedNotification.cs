namespace Hand2Note.ProgressView.ViewModel.Progress.Notifications
{
    public class PausedNotification : IProgressNotification
    {
        public PausedNotification(string caption)
        {
            Caption = caption;
        }

        public string Caption { get; }
        public bool AllowPause => false;
    }
}