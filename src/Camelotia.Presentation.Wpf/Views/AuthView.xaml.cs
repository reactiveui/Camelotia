using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Wpf.Views
{
    public partial class AuthView : UserControl, IViewFor<IAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IAuthViewModel), typeof(AuthView), null);

        public AuthView()
        {
            InitializeComponent();
            DataContextChanged += (sender, args) => ViewModel = DataContext as IAuthViewModel;
            this.WhenActivated(disposable =>
            {
                this.WhenAnyValue(x => x.ViewModel.SupportsDirectAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => AuthTabs.SelectedIndex = 0)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.ViewModel.SupportsHostAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => AuthTabs.SelectedIndex = 1)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.ViewModel.SupportsOAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => AuthTabs.SelectedIndex = 2)
                    .DisposeWith(disposable);
            });
        }

        public IAuthViewModel ViewModel
        {
            get => (IAuthViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IAuthViewModel)value;
        }
    }
}
