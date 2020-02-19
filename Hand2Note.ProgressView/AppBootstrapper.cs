using System.Management.Instrumentation;
using System.Reflection;
using Hand2Note.ProgressView.View;
using Hand2Note.ProgressView.ViewModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Hand2Note.ProgressView
{
    public class AppBootstrapper
    {
        public AppBootstrapper()
        {
            RegisterDeps(Locator.CurrentMutable);
            MainViewModel = Locator.Current.GetService<MainViewModel>();
        }
        
        public MainViewModel MainViewModel { get; set; }

        private void RegisterDeps(IMutableDependencyResolver dependencyResolver)
        {
            dependencyResolver.InitializeSplat();
            dependencyResolver.InitializeReactiveUI();
            
            dependencyResolver.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
            dependencyResolver.RegisterConstant(new MainViewModel());
        }
    }
}