using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class MainView : Page, IViewFor<IMainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IMainViewModel), typeof(MainView), null);

        public MainView()
        {
            ViewModel = Bootstrapper.BuildMainViewModel();
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        public IMainViewModel ViewModel
        {
            get => (IMainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IMainViewModel)value;
        }
    }
}
