using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed partial class OAuthView : ReactiveUserControl<IOAuthViewModel>
    {
        public OAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}
