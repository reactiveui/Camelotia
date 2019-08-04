using Camelotia.Presentation.Interfaces;
using MahApps.Metro.Controls;
using ReactiveUI;
using System.Windows;

namespace Camelotia.Presentation.Wpf
{
    public partial class MainView : MetroWindow, IViewFor<IMainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IMainViewModel), typeof(MainView), null);

        public MainView()
        {
            InitializeComponent();
            DataContext = ViewModel = new Bootstrapper().MainViewModel;
            this.WhenActivated(disposable => { });
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
