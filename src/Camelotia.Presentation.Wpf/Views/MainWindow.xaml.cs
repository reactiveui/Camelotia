using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf
{
    public partial class MainWindow : ReactiveWindow<IMainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(disposable => { });
        }
    }
}
