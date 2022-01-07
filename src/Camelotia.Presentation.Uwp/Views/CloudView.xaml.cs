using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views;

public sealed partial class CloudView : UserControl, IViewFor<ICloudViewModel>
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty
        .Register(nameof(ViewModel), typeof(ICloudViewModel), typeof(CloudView), null);

    public CloudView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
    }

    public ICloudViewModel ViewModel
    {
        get => (ICloudViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (ICloudViewModel)value;
    }
}
