using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Infrastructure;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Wpf.Views;

public partial class DirectAuthView : UserControl, IViewFor<IDirectAuthViewModel>
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty
        .Register(nameof(ViewModel), typeof(IDirectAuthViewModel), typeof(DirectAuthView), null);

    public DirectAuthView()
    {
        InitializeComponent();
        DataContextChanged += (sender, args) => ViewModel = DataContext as IDirectAuthViewModel;
        this.WhenActivated((ContainerDisposable disposable) => { });
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
