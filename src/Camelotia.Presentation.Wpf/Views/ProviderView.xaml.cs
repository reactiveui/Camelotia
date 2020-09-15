using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
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
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel,
                        vm => vm.ShowBreadCrumbs,
                        view => view.PathTextBlock.Visibility,
                        showBreadCrumbs => showBreadCrumbs ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        vm => vm.ShowBreadCrumbs,
                        view => view.BreadCrumbsListBox.Visibility,
                        showBreadCrumbs => showBreadCrumbs ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
            });
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
