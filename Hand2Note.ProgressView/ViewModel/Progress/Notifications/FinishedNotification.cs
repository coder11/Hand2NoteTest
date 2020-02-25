namespace Hand2Note.ProgressView.ViewModel.Progress.Notifications
{
    public class FinishedNotification : IProgressNotification
    {
        public FinishedNotification(string caption)
        {
            Caption = caption;
        }

        public string Caption { get; }
        public bool AllowPause => false;
    }
}