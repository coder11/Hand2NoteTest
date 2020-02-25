using System;
using System.Reactive.Linq;
using Hand2Note.ProgressView.ViewModel.Progress;
using Hand2Note.ProgressView.ViewModel.Progress.Notifications;

namespace Hand2Note.ProgressView.Model.DownloadMe
{
    public static class DownloadMeProgressViewAdapter
    {
        private const string Connecting = "Connecting";
        private const string Downloading = "Downloading";
        private const string Finishing = "Finishing";
        private const string Paused = "Paused";
        private const string Pausing = "Pausing";
        private const string Finished = "Finished";
        
        public static IObservable<IProgressNotification> FsmStatesToNotifications(DownloadMeFsm fsm)
        {
            var stateInfos = fsm.States
                .Select<DownloadMeState, IProgressNotification>(x =>
                {
                    switch (x.State)
                    {
                        case DownloadMeStateType.Initial:
                        case DownloadMeStateType.Starting:
                            return new ProgressInitNotification(0, fsm.TotalBytesToDownload);
                        
                        case DownloadMeStateType.Connecting:
                            return new ProgressLessNotification(Connecting, true);
                        
                        case DownloadMeStateType.Connected:
                            return new ProgressNotification(x.Progress, 0, fsm.TotalBytesToDownload, Downloading, true);
                        
                        case DownloadMeStateType.Downloading:
                            return new ProgressNotification(x.Progress, x.ProgressIncrement!.Value, fsm.TotalBytesToDownload, Downloading, true);
                        
                        case DownloadMeStateType.Pausing:
                            return new ProgressLessNotification(Pausing, true);
                        
                        case DownloadMeStateType.Paused:
                            return new PausedNotification(Paused);
                        
                        
                        case DownloadMeStateType.Finishing:
                            return new ProgressLessNotification(Finishing, true);
                        
                        case DownloadMeStateType.Finished:
                            return new FinishedNotification(Finished);
                        
                        default:
                            throw new ArgumentException();
                    }
                });

            return stateInfos;
        }
    }
}