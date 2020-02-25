using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace Hand2Note.ProgressView.Model.DownloadMe
{
    public class DownloadMeFsm : IDisposable
    {
        private readonly ThirdParty.DownloadMe _downloadMe;

        private DownloadMeState _state;
        
        private readonly Subject<DownloadMeState> _subject = new Subject<DownloadMeState>();

        public static DownloadMeFsm Create()
        {
            var result = new DownloadMeFsm(new ThirdParty.DownloadMe());
            result._state = new DownloadMeState(
                DownloadMeStateType.Initial,
                0,
                null,
                new CancellationTokenSource());

            return result;
        }
        
        public IObservable<DownloadMeState> States => _subject;

        public int TotalBytesToDownload => _downloadMe.TotalBytesToDownload;
        
        public void OnStart()
        {
            switch (_state.State)
            {
                case DownloadMeStateType.Initial:
                case DownloadMeStateType.Finished:
                    var token = new CancellationTokenSource();
                    var newState = new DownloadMeState(
                        DownloadMeStateType.Starting,
                        0,
                        null,
                        token);
                        
                    ChangeState(newState);
                    Task.Run(() => _downloadMe.StartDownload(token.Token, _state.Progress));
                    break;
                    
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }

        public void OnPause()
        {
            switch (_state.State)
            {
                case DownloadMeStateType.Connecting:
                case DownloadMeStateType.Downloading:
                case DownloadMeStateType.Finishing:
                    _state.Token.Cancel();
                    ChangeState(_state.UpdateType(DownloadMeStateType.Pausing));
                    break;
                    
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }
        
        public void OnResume()
        {
            switch (_state.State)
            {
                case DownloadMeStateType.Paused:
                    var newToken = new CancellationTokenSource();
                    var newState = _state.UpdateToken(newToken)
                        .UpdateType(DownloadMeStateType.Starting);
                    ChangeState(newState);
                    Task.Run(() => _downloadMe.StartDownload(newToken.Token, _state.Progress));
                    break;
                    
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }
        
        private DownloadMeFsm(ThirdParty.DownloadMe downloadMe)
        {
            _downloadMe = downloadMe;

            _downloadMe.Connected += OnConnected;
            _downloadMe.Connecting += OnConnecting;
            _downloadMe.Finished += OnFinished;
            _downloadMe.Finishing += OnFinishing;
            _downloadMe.Paused += OnPaused;
            _downloadMe.BytesReceived += OnBytesReceived;
        }

        private void ChangeState(DownloadMeState newState)
        {
            _state = newState;
            _subject.OnNext(newState);
        }
        
        private void OnPaused()
        {
            switch (_state.State)
            {
                case DownloadMeStateType.Pausing:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Paused));
                    break;
                    
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }

        private void OnBytesReceived(int bytesAmount)
        {
            switch (_state.State)
            {
                case  DownloadMeStateType.Connected:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Downloading)
                        .UpdateProgress(bytesAmount));
                    break;
                    
                case DownloadMeStateType.Downloading:
                case DownloadMeStateType.Pausing:
                    ChangeState(_state.UpdateProgress(bytesAmount));
                    break;
                
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }

        private void OnFinishing()
        {
            switch (_state.State)
            {
                case  DownloadMeStateType.Downloading:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Finishing));
                    break;
                    
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }

        private void OnFinished()
        {
            switch (_state.State)
            {
                case  DownloadMeStateType.Finishing:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Finished));
                    break;
                
                case DownloadMeStateType.Pausing:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Paused));
                    break;
                    
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }

        private void OnConnecting()
        {
            switch (_state.State)
            {
                case DownloadMeStateType.Initial:
                case DownloadMeStateType.Starting:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Connecting));
                    break;
                
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }

        private void OnConnected()
        {
            switch (_state.State)
            {
                case DownloadMeStateType.Connecting:
                    ChangeState(_state.UpdateType(DownloadMeStateType.Connected));
                    break;
                
                case DownloadMeStateType.Pausing:
                    break;
                
                default:
                    ThrowInvalidStateTransition();
                    break;
            }
        }
        
        private void ThrowInvalidStateTransition()
        {
            throw new InvalidOperationException("Invalid downloadMe state transition");
        }

        public void Dispose()
        {
            _subject?.Dispose();
        }
    }
}