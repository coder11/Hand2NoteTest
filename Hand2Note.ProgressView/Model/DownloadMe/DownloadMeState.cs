using System.Threading;

namespace Hand2Note.ProgressView.Model.DownloadMe
{
    public class DownloadMeState
    {
        public DownloadMeState(DownloadMeStateType state, int totalProgress, int? progressIncrement, CancellationTokenSource token)
        {
            State = state;
            TotalProgress = totalProgress;
            ProgressIncrement = progressIncrement;
            Token = token;
        }

        public DownloadMeState UpdateProgress(int increment)
        {
            return new DownloadMeState(
                State,
                TotalProgress + increment,
                increment,
                Token);
        }

        public DownloadMeState UpdateType(DownloadMeStateType newType)
        {
            return new DownloadMeState(
                newType,
                TotalProgress,
                null,
                Token);
        }
        
        public DownloadMeState UpdateToken(CancellationTokenSource newToken)
        {
            return new DownloadMeState(
                State,
                TotalProgress,
                ProgressIncrement,
                newToken);
        }

        public DownloadMeStateType State { get; }
        public int TotalProgress { get; }
        public int? ProgressIncrement { get; }
        public CancellationTokenSource Token { get; }
    }
}