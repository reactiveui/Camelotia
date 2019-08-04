using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using System.Windows;
using System.Windows.Controls;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class CreateFolderView : UserControl, IViewFor<ICreateFolderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(ICreateFolderViewModel), typeof(CreateFolderView), null);

        public CreateFolderView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as ICreateFolderViewModel;
            this.WhenActivated(disposable => { });
        }

        public ICreateFolderViewModel ViewModel
        {
            get => (ICreateFolderViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ICreateFolderViewModel)value;
        }
    }
}
