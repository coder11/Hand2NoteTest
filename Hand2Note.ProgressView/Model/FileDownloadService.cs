using System;
using System.ComponentModel;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace Hand2Note.ProgressView.Model
{
    public class FileDownloadingProgress
    {
        public long BytesReceived { get; set; }
        public long TotalBytesToReceive { get; set; }
        public int ProgressPercentage { get; set; }
        public long TotalBytes { get; set; }
    }

    public class FileDownloadingThing
    {
        public IObservable<FileDownloadingProgress> Progress { get; set; }
        public IObservable<Unit> Finished { get; set; }
    }
    
    public class FileDownloadService : IFileDownloadService
    {
        private const string url = "http://h2n-uptoyou.azureedge.net/main/Hand2NoteInstaller.exe";
        
        public FileDownloadingThing DownloadFile()
        {
            var client = new WebClient();
            client.DownloadFileTaskAsync(url, GetTempFile());

            var progress = Observable.FromEventPattern<DownloadProgressChangedEventHandler, DownloadProgressChangedEventArgs>(
                x => client.DownloadProgressChanged += x,
                x => client.DownloadProgressChanged -= x,
                RxApp.TaskpoolScheduler);

            var finished = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>(
                x => client.DownloadFileCompleted += x,
                x => client.DownloadFileCompleted -= x,
                RxApp.TaskpoolScheduler);
            
            return new FileDownloadingThing
            {
                Progress = progress.Select(x => new FileDownloadingProgress
                {
                    BytesReceived = x.EventArgs.BytesReceived,
                    TotalBytesToReceive = x.EventArgs.TotalBytesToReceive,
                    ProgressPercentage = x.EventArgs.ProgressPercentage,
                }),
                Finished = finished.Select(x => Unit.Default)
            };
        }

        private string GetTempFile()
        {
            return System.IO.Path.GetTempPath() + Guid.NewGuid();
        }
    }

    public interface IFileDownloadService
    {
    }
}