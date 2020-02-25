namespace Hand2Note.ProgressView.ViewModel.Progress
{
    public class ProgressNotification : BaseProgressNotification
    {
        public ProgressNotification(int progress, int? progressIncrement, int progressMaxValue, string caption, bool allowPause) 
            : base(progress, 
                progressIncrement, 
                progressMaxValue, 
                false, 
                allowPause, 
                false, 
                false, 
                false, 
                false, 
                false, 
                caption)
        {
        }
    }

    public class ProgressLessNotification : BaseProgressNotification
    {
        public ProgressLessNotification(string caption, bool allowPause) 
            : base(0, 
                null, 
                0, 
                false, 
                allowPause, 
                false, 
                true, 
                false, 
                true, 
                true, 
                caption)
        {
        }
    }
    
    public class FinishNotification : BaseProgressNotification
    {
        public FinishNotification(int progress, string caption, bool hideProgressText = false) 
            : base(progress, null, progress, true, false, false, false, hideProgressText, true, false, caption)
        {
        }
    }
    
    public class PausedNotification : BaseProgressNotification
    {
        public PausedNotification(int progress, int progressMaxValue, string caption) 
            : base(progress, 
                null, 
                progressMaxValue, 
                false, 
                false, 
                true, 
                true, 
                false, 
                true, 
                false, 
                caption)
        {
        }
    }
}