using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class FolderView : UserControl, IViewFor<IFolderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IFolderViewModel), typeof(FolderView), null);

        public FolderView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IFolderViewModel;
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.TopLevelMenu.IsSubmenuOpen)
                    .Select(isOpen => !isOpen)
                    .BindTo(this, x => x.ChevronRightIcon.Visibility)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.TopLevelMenu.IsSubmenuOpen)
                    .BindTo(this, x => x.ChevronDownIcon.Visibility)
                    .DisposeWith(disposables);
            });
        }

        public IFolderViewModel ViewModel
        {
            get => (IFolderViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IFolderViewModel)value;
        }
    }
}
