using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class OAuthView : UserControl, IViewFor<IOAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IOAuthViewModel), typeof(OAuthView), null);

        public OAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        public IOAuthViewModel ViewModel
        {
            get => (IOAuthViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IOAuthViewModel)value;
        }
    }
}
