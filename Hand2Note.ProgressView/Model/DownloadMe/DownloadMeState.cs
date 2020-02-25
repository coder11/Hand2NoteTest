using System.Threading;

namespace Hand2Note.ProgressView.Model.DownloadMe
{
    public class DownloadMeState
    {
        public DownloadMeState(DownloadMeStateType state, int progress, int? progressIncrement,
            CancellationTokenSource token)
        {
            State = state;
            Progress = progress;
            ProgressIncrement = progressIncrement;
            Token = token;
        }

        public DownloadMeStateType State { get; }
        public int Progress { get; }
        public int? ProgressIncrement { get; }
        public CancellationTokenSource Token { get; }

        public DownloadMeState UpdateProgress(int increment)
        {
            return new DownloadMeState(
                State,
                Progress + increment,
                increment,
                Token);
        }

        public DownloadMeState UpdateType(DownloadMeStateType newType)
        {
            return new DownloadMeState(
                newType,
                Progress,
                null,
                Token);
        }

        public DownloadMeState UpdateToken(CancellationTokenSource newToken)
        {
            return new DownloadMeState(
                State,
                Progress,
                ProgressIncrement,
                newToken);
        }
    }
}