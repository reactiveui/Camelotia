using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class ProviderView : ReactiveUserControl<IProviderViewModel>
    {
        public ProviderView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IProviderViewModel;
            this.WhenActivated(disposables => { });
        }
    }
}
