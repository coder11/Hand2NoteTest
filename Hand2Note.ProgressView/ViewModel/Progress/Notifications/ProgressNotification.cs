namespace Hand2Note.ProgressView.ViewModel.Progress.Notifications
{
    public class ProgressNotification : IProgressNotification
    {
        public ProgressNotification(int progress, 
            int progressIncrement, 
            int progressMaxValue, 
            string caption, 
            bool allowPause)
        {
            Progress = progress;
            ProgressIncrement = progressIncrement;
            ProgressMaxValue = progressMaxValue;
            Caption = caption;
            AllowPause = allowPause;
        }

        public int Progress { get; }
        public int ProgressIncrement { get; }
        public int ProgressMaxValue { get; }
        
        public string Caption { get; }
        public bool AllowPause { get; }
    }
}