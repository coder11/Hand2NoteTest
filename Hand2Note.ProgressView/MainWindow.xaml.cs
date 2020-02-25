namespace Hand2Note.ProgressView
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            AppBootstrapper = new AppBootstrapper();

            Main.ViewModel = AppBootstrapper.MainViewModel;
        }

        public AppBootstrapper AppBootstrapper { get; }
    }
}