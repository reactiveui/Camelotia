using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Xamarin.Views;
using Plugin.Iconize.Fonts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Camelotia.Presentation.Xamarin
{
    public partial class App : Application
    {
        public App(IMainViewModel viewModel)
        {
            Plugin.Iconize.Iconize
                .With(new FontAwesomeRegularModule())
                .With(new FontAwesomeSolidModule())
                .With(new FontAwesomeBrandsModule());

            InitializeComponent();
            MainPage = new MainView { ViewModel = viewModel };
        }
    }
}
