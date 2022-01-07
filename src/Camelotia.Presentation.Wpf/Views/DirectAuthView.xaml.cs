using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views;

public partial class DirectAuthView : UserControl, IViewFor<IDirectAuthViewModel>
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty
        .Register(nameof(ViewModel), typeof(IDirectAuthViewModel), typeof(DirectAuthView), null);

    public DirectAuthView()
    {
        InitializeComponent();
        DataContextChanged += (sender, args) => ViewModel = DataContext as IDirectAuthViewModel;
        this.WhenActivated(disposable => { });
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
