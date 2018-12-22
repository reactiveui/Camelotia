using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class DirectAuthView : UserControl, IViewFor<IDirectAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IDirectAuthViewModel), typeof(DirectAuthView), null);

        public DirectAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        public IDirectAuthViewModel ViewModel
        {
            get => (IDirectAuthViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IDirectAuthViewModel)value;
        }
    }
}
