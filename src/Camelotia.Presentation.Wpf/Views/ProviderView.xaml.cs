using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using System.Windows;
using System.Windows.Controls;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class ProviderView : UserControl, IViewFor<IProviderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IProviderViewModel), typeof(ProviderView), null);

        public ProviderView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IProviderViewModel;
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
