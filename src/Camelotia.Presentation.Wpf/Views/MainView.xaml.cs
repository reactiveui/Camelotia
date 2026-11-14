using System.Windows;
using Camelotia.Presentation.Interfaces;
using MahApps.Metro.Controls;
using Camelotia.Presentation.Infrastructure;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Wpf
{
    public partial class MainView : MetroWindow, IViewFor<IMainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IMainViewModel), typeof(MainView), null);

        public MainView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IMainViewModel;
            this.WhenActivated((ContainerDisposable disposable) => { });
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
