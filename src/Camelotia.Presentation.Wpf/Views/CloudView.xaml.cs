using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class CloudView : UserControl, IViewFor<ICloudViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(ICloudViewModel), typeof(CloudView), null);

        public CloudView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as ICloudViewModel;
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(
                    ViewModel,
                    vm => vm.ShowBreadCrumbs,
                    view => view.PathTextBlock.Visibility,
                    showBreadCrumbs => showBreadCrumbs ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(disposables);

                this.OneWayBind(
                    ViewModel,
                    vm => vm.ShowBreadCrumbs,
                    view => view.BreadCrumbsListBox.Visibility,
                    showBreadCrumbs => showBreadCrumbs ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
            });
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
}
