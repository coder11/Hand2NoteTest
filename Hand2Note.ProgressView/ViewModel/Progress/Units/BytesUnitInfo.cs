using System;

namespace Hand2Note.ProgressView.ViewModel.Progress.Units
{
    public class BytesUnitInfo : IUnitInfo
    {
        public string GetPresentableText(int byteCount)
        {
            string[] suf = {"B", "KB", "MB", "GB", "TB", "PB"};
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Math.Sign(byteCount) * num + suf[place];
        }
    }
}