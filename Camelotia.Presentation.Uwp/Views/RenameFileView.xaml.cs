using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class RenameFileView : UserControl, IViewFor<IRenameFileViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IRenameFileViewModel), typeof(RenameFileView), null);

        public RenameFileView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        public IRenameFileViewModel ViewModel
        {
            get => (IRenameFileViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IRenameFileViewModel)value;
        }
    }
}
