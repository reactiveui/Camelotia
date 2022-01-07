using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class FileView : UserControl, IViewFor<IFileViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(IFileViewModel), typeof(FileView), null);

        public FileView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.Events()
                    .RightTapped
                    .Select(args => this)
                    .Do(sender => sender.ViewModel.Provider.SelectedFile = sender.ViewModel)
                    .Subscribe(FlyoutBase.ShowAttachedFlyout)
                    .DisposeWith(disposables);

                this.Events()
                    .DoubleTapped
                    .Select(args => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel.Provider.Open)
                    .DisposeWith(disposables);
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
