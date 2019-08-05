using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using System.Windows;
using System.Windows.Controls;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class RenameFileView : UserControl, IViewFor<IRenameFileViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IRenameFileViewModel), typeof(RenameFileView), null);

        public RenameFileView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IRenameFileViewModel;
            this.WhenActivated(disposable => { });
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
