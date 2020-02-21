using System;
using System.Text;

namespace Hand2Note.ProgressView.Model.Operations
{
    public interface IUnitInfo
    {
        string GetPresentableText(int value);
    }

    public class BytesUnitInfo : IUnitInfo
    {
        public string GetPresentableText(int byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }
    }

    public interface IProgressStage
    {
        IObservable<int> Progress { get; }
        int? ProgressMax { get; }
        IUnitInfo UnitInfo { get; }
        string Caption { get; }
        bool IsProgressless { get; }
        bool CanPause { get; }
        
        bool HasFinished { get; }
    }
    
    public sealed class ProgressStage : IProgressStage
    {
        public ProgressStage(IObservable<int> progress, int progressMax, IUnitInfo unitInfo, string caption, bool canPause = false)
        {
            Progress = progress;
            ProgressMax = progressMax;
            UnitInfo = unitInfo;
            Caption = caption;
            CanPause = canPause;
        }

        public IObservable<int> Progress { get; }
        public int? ProgressMax { get; }
        public IUnitInfo UnitInfo { get; }
        public string Caption { get; }
        public bool IsProgressless => false;
        public bool CanPause { get; }
        public bool HasFinished => false;
    }
    
    public sealed class ProgressLessStage : IProgressStage
    {
        public ProgressLessStage(string caption, bool canPause = false)
        {
            Caption = caption;
            CanPause = canPause;
        }

        public IObservable<int> Progress => null;
        public int? ProgressMax => null;
        public IUnitInfo UnitInfo => null;
        public string Caption { get; }
        public bool IsProgressless => true;
        public bool CanPause { get; }
        public bool HasFinished => false;
    }
    
    public sealed class FinishStage : IProgressStage
    {
        public FinishStage(string caption)
        {
            Caption = caption;
        }
        
        public bool HasFinished => true;

        public IObservable<int> Progress => null;
        public int? ProgressMax => null;
        public IUnitInfo UnitInfo => null;
        public string Caption { get; }
        public bool IsProgressless => false;
        public bool CanPause => false;

    }
}