using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class ProviderView : UserControl, IViewFor<IProviderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IProviderViewModel), typeof(ProviderView), null);

        public ProviderView()
        {
            InitializeComponent();
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
        
        private void OnFileViewTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            ViewModel.SelectedFile = (FileModel) element.DataContext;
            FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}
