using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed partial class ProviderView : ReactiveUserControl<ICloudViewModel>
    {
        public ProviderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}
