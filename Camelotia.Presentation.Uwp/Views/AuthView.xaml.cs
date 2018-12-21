using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class AuthView : UserControl, IViewFor<IAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IAuthViewModel), typeof(AuthView), null);

        public AuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.WhenAnyValue(x => x.ViewModel.SupportsDirectAuth)
                    .Select(supportsDirectAuth => supportsDirectAuth ? 0 : 1)
                    .BindTo(this, x => x.AuthorizationPivot.SelectedIndex)
                    .DisposeWith(disposables);
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
