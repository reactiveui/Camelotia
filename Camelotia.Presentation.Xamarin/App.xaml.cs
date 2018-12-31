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
        	Plugin.Iconize.Iconize
                .With(new Plugin.Iconize.Fonts.FontAwesomeRegularModule())
                .With(new Plugin.Iconize.Fonts.FontAwesomeSolidModule())
                .With(new Plugin.Iconize.Fonts.FontAwesomeBrandsModule());

            InitializeComponent();
            MainPage = new MainView { ViewModel = viewModel };
        }
    }
}
