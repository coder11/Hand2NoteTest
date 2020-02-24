namespace Hand2Note.ProgressView.Model.Progress
{
    public interface IProgressNotification
    {
        int Progress { get; }
        int? ProgressIncrement { get; }
        int ProgressMaxValue { get; }
        bool HasFinished { get; }
        bool AllowPause { get; }
        bool AllowResume { get; }
        bool HideSpeed { get; }
        bool HideProgressText { get; }
        bool HideRemainingTime { get; }
        bool DisplayAsProgressLess { get; }
        string Caption { get; }
    }
}