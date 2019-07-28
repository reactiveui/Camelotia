using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class HostAuthView : UserControl, IViewFor<IHostAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IHostAuthViewModel), typeof(DirectAuthView), null);

        public HostAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        public IHostAuthViewModel ViewModel
        {
            get => (IHostAuthViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IHostAuthViewModel)value;
        }
    }
}
