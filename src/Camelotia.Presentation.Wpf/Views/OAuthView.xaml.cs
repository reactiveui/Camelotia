using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using System.Windows;
using System.Windows.Controls;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class OAuthView : UserControl, IViewFor<IOAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IOAuthViewModel), typeof(OAuthView), null);

        public OAuthView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IOAuthViewModel;
            this.WhenActivated(disposable => { });
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
