namespace Hand2Note.ProgressView.ViewModel.Progress.Units
{
    public class FilesUnitInfo : IUnitInfo
    {
        public string GetPresentableText(int value)
        {
            if (value == 1)
                return "1 file";

            return $"{value} files";
        }
    }
}