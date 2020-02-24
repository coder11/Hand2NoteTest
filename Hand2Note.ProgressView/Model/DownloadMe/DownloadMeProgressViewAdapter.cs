using System;
using System.Reactive.Linq;
using Hand2Note.ProgressView.Util;
using Hand2Note.ProgressView.ViewModel.Progress;

namespace Hand2Note.ProgressView.Model.DownloadMe
{
    public static class DownloadMeProgressViewAdapter
    {
        private const string Connecting = "Connecting";
        private const string Downloading = "Downloading";
        private const string Finishing = "Finishing";
        private const string Paused = "Paused";
        private const string Finished = "Finished";
        
        public static IObservable<ProgressNotification> FsmStatesToNotifications(DownloadMeFsm fsm)
        {
            var stateInfos = fsm.States
                .Select(x =>
                {
                    var isProgressless = x.State.In(
                        DownloadMeStateType.Connecting,
                        DownloadMeStateType.Finishing,
                        DownloadMeStateType.Pausing,
                        DownloadMeStateType.Starting);

                    string caption = string.Empty;
                    switch (x.State)
                    {
                        case DownloadMeStateType.Starting:
                        case DownloadMeStateType.Connecting:
                            caption = Connecting;
                            break;

                        case DownloadMeStateType.Downloading:
                            caption = Downloading;
                            break;

                        case DownloadMeStateType.Finishing:
                            caption = Finishing;
                            break;

                        case DownloadMeStateType.Paused:
                            caption = Paused;
                            break;

                        case DownloadMeStateType.Finished:
                            caption = Finished;
                            break;
                    }

                    bool allowPause = x.State.In(
                        DownloadMeStateType.Connecting,
                        DownloadMeStateType.Downloading,
                        DownloadMeStateType.Finishing);

                    bool hasFinished = x.State == DownloadMeStateType.Finished;

                    bool allowResume = x.State == DownloadMeStateType.Paused;

                    bool hideRemainingTime = x.State != DownloadMeStateType.Downloading;
                    
                    return new ProgressNotification(
                        x.Progress,
                        x.ProgressIncrement,
                        fsm.TotalBytesToDownload,
                        hasFinished,
                        allowPause,
                        allowResume,
                        false,
                        false,
                        hideRemainingTime,
                        isProgressless,
                        caption);
                });

            return stateInfos;
        }
    }
}