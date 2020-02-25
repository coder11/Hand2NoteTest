using System.Globalization;

namespace Hand2Note.ProgressView.ViewModel.Progress.Units
{
    public class BareUnitInfo : IUnitInfo
    {
        public string GetPresentableText(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}