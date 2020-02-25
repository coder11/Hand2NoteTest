namespace Hand2Note.ProgressView.ViewModel.Progress
{
    public class BaseProgressNotification : IProgressNotification
    {
        public BaseProgressNotification(int progress, int? progressIncrement, int progressMaxValue, bool hasFinished,
            bool allowPause, bool allowResume, bool hideSpeed, bool hideProgressText, bool hideRemainingTime,
            bool displayAsProgressLess, string caption)
        {
            Progress = progress;
            ProgressIncrement = progressIncrement;
            ProgressMaxValue = progressMaxValue;
            HasFinished = hasFinished;
            AllowPause = allowPause;
            AllowResume = allowResume;
            HideSpeed = hideSpeed;
            HideProgressText = hideProgressText;
            HideRemainingTime = hideRemainingTime;
            DisplayAsProgressLess = displayAsProgressLess;
            Caption = caption;
        }

        public int Progress { get; }
        public int? ProgressIncrement { get; }
        public int ProgressMaxValue { get; }
        public bool HasFinished { get; }
        public bool AllowPause { get; }
        public bool AllowResume { get; }
        public bool HideSpeed { get; }
        public bool HideProgressText { get; }
        public bool HideRemainingTime { get; }
        public bool DisplayAsProgressLess { get; }
        public string Caption { get; }
    }
}