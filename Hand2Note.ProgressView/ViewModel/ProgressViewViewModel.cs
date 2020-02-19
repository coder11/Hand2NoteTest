using System.Reactive;
using System.Reactive.Linq;
using Hand2Note.ProgressView.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Hand2Note.ProgressView.ViewModel
{
    public class ProgressViewModel : ReactiveObject
    {
        private FileDownloadService _dloader = new FileDownloadService();
        
        public ProgressViewModel()
        {
            var canExecute = this.WhenAny(
                x => x.ExecutionStarted,
                x => x.ExecutionFinished,
                (started, finished) => !started.Value || finished.Value);
            
            Command = ReactiveCommand.Create(() =>
            {
                var res = _dloader.DownloadFile();
                return res;
            }, canExecute);


            Command.SelectMany(x => x.Progress)
                .Select(x => x.ProgressPercentage)
                .ToPropertyEx(this, x => x.ProgressValue);
            
            Command.SelectMany(x => x.Finished)
                .Select(x => true)
                .StartWith(false)
                .ToPropertyEx(this, x => x.ExecutionFinished);

            Command.Select(x => true)
                .StartWith(false)
                .ToPropertyEx(this, x => x.ExecutionStarted);
        }

        [Reactive] public long ProgressMax { get; set; } = 100;

        public int ProgressValue { [ObservableAsProperty] get;  }
        
        public bool ExecutionStarted { [ObservableAsProperty] get;  }
        
        public bool ExecutionFinished { [ObservableAsProperty] get;  }

        [Reactive] 
        public string ButtonText { get; set; } = "Button";

        [Reactive] 
        public string Caption { get; set; } = "Downloading";
        
        [Reactive]
        public ReactiveCommand<Unit, FileDownloadingThing> Command { get; set; }
    }
}