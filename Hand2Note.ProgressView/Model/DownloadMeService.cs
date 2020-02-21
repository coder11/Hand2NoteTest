using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hand2Note.ProgressView.Model.Operations;

namespace Hand2Note.ProgressView.Model
{
    public class DownloadMeState
    {
        public DownloadMe DownloadMe { get; set; }
        public int Progress { get; set; }
        public CancellationTokenSource CancelToken { get; set; }
    }

    public interface IOperationWithProgressService
    {
        (object State, IObservable<IProgressStage> Notification) Init();
        object Start(object state);
        object Pause(object state);
        object Resume(object state);
    }
    
    public class DownloadMeService : IOperationWithProgressService
    {
        
        public (object State, IObservable<IProgressStage> Notification) Init()
        {
            var downloadMe = new DownloadMe();
            var (stages, progress) = CreateOperations(downloadMe);

            var state = new DownloadMeState
            {
                Progress = 0,
                DownloadMe = downloadMe
            };

            progress.Subscribe(x => { state.Progress += x; });

            return (state, stages);
        }
        
        public object Start(object state)
        {
            var s = (DownloadMeState) state;
            s.CancelToken = new CancellationTokenSource();
            
            Task.Run(() => s.DownloadMe.StartDownload(s.CancelToken.Token, s.Progress));

            return s;
        }

        public object Pause(object state)
        {
            var s = (DownloadMeState) state;
            s.CancelToken.Cancel();
            return s;
        }

        public object Resume(object state)
        {
            return Start(state);
        }
        
        private (IObservable<IProgressStage>, IObservable<int>) CreateOperations(DownloadMe downloadMe)
        {
            var finishing = Observable.FromEvent(
                h => downloadMe.Finishing += h,
                h => downloadMe.Finishing -= h);
            
            var finished = Observable.FromEvent(
                h => downloadMe.Finished += h,
                h => downloadMe.Finished -= h);
            
            var connecting = Observable.FromEvent(
                h => downloadMe.Connecting += h,
                h => downloadMe.Connecting -= h);
            
            var connected = Observable.FromEvent(
                h => downloadMe.Connected += h,
                h => downloadMe.Connected -= h);
            
            var paused = Observable.FromEvent(
                h => downloadMe.Paused += h,
                h => downloadMe.Paused -= h);

            var progress = Observable.FromEvent<int>(
                h => downloadMe.BytesReceived += h,
                h => downloadMe.BytesReceived -= h);

            var total = downloadMe.TotalBytesToDownload;

            var result = Observable.Merge(
                connecting
                    .Select(_ => new ProgressLessStage("Connecting", canPause: true))
                    .Select(x => x as IProgressStage)
                    .TakeUntil(connected),

                connected
                    .Select(x => new ProgressStage(progress,
                        total,
                        new BytesUnitInfo(),
                        "Downloading", canPause: true))
                    .TakeUntil(finishing),

                finishing
                    .Select(x => new ProgressLessStage("Finishing", canPause: true))
                    .TakeUntil(finished),
                
                finished
                    .Select(x => new FinishStage("Finished")));

            return (result, progress);
        }
    }
}