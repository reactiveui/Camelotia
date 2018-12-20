using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class ProviderView : UserControl, IViewFor<IProviderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IProviderViewModel), typeof(MainView), null);

        public ProviderView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = (IProviderViewModel)args.NewValue;
            this.WhenActivated(disposables => { });
        }

        public IProviderViewModel ViewModel
        {
            get => (IProviderViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IProviderViewModel)value;
        }
    }
}
