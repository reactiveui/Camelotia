using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

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
                var controlRightTapped = Observable
                    .FromEventPattern<RightTappedEventHandler, RightTappedRoutedEventArgs>(
                        handler => RightTapped += handler,
                        handler => RightTapped -= handler)
                    .Select(args => (FileView) args.Sender);

                controlRightTapped
                    .Select(element => element.ViewModel)
                    .Subscribe(file => file.Provider.SelectedFile = file)
                    .DisposeWith(disposables);

                controlRightTapped
                    .Subscribe(FlyoutBase.ShowAttachedFlyout)
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
