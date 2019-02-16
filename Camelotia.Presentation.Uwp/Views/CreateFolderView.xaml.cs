using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class CreateFolderView : UserControl, IViewFor<ICreateFolderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(ICreateFolderViewModel), typeof(CreateFolderView), null);

        public CreateFolderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
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
