namespace Hand2Note.ProgressView.ViewModel.Progress
{
    public class ProgressViewModelConfig
    {
        public string PauseButtonText { get; set; } = "Pause";
        public string ResumeButtonText { get; set; } = "Resume";
        public string StartButtonText { get; set; } = "Start";
        public string RestartButtonText { get; set; } = "Restart";
        public IUnitInfo Units { get; set; } = new BareUnitInfo();

        public string SpeedTextTemplate { get; set; } = "Speed: {0}/s";

        public string ProgressTextTemplate { get; set; } = "Downloaded: {0} / {1}";

        public string RemainingTimeTextTemplate { get; set; } = "Time remaining: {0:mm\\:ss}";
    }
}