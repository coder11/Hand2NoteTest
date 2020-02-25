namespace Hand2Note.ProgressView.ViewModel.Progress
{
    public interface IProgressNotification
    {
        string Caption { get; }
        bool AllowPause { get; }
    }
}