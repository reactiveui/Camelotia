using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class FileView : UserControl, IViewFor<IFileViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IFileViewModel), typeof(FileView), null);

        public FileView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IFileViewModel;
            this.WhenActivated(disposable =>
            {
                this.Events()
                    .MouseDoubleClick
                    .Select(args => Unit.Default)
                    .InvokeCommand(ViewModel.Provider.Open)
                    .DisposeWith(disposable);
            });
        }

        public IFileViewModel ViewModel
        {
            get => (IFileViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IFileViewModel)value;
        }
    }
}
