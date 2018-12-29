using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Xamarin.View;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Camelotia.Presentation.Xamarin
{
    public partial class App : Application
    {
        public App(IMainViewModel viewModel)
        {
            InitializeComponent();
            MainPage = new MainView { ViewModel = viewModel };
        }
    }
}
