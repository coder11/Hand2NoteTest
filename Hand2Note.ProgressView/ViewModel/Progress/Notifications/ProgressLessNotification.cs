namespace Hand2Note.ProgressView.ViewModel.Progress.Notifications
{
    public class ProgressLessNotification : IProgressNotification
    {
        public ProgressLessNotification(string caption, bool allowPause)
        {
            Caption = caption;
            AllowPause = allowPause;
        }

        public string Caption { get; }
        public bool AllowPause { get; }
    }
}