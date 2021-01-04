using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class HostAuthView : UserControl, IViewFor<IHostAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IHostAuthViewModel), typeof(HostAuthView), null);

        public HostAuthView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IHostAuthViewModel;
            this.WhenActivated(disposable => { });
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
