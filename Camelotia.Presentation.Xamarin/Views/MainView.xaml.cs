using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ReactiveMasterDetailPage<IMainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.SelectedProvider)
                    .Select(provider => false)
                    .Subscribe(x => IsPresented = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SelectedProvider)
                    .Where(provider => provider != null)
                    .Select(x => new ProviderView { ViewModel = ViewModel.SelectedProvider })
                    .Subscribe(view => NavigationView.PushAsync(view))
                    .DisposeWith(disposables);
            });
        }
    }
}