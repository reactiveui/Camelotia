using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System;

namespace Camelotia.Presentation.Xamarin.View
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
                    .BindTo(ProviderView, x => x.ViewModel)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SelectedProvider)
                    .Select(provider => false)
                    .Subscribe(x => IsPresented = x)
                    .DisposeWith(disposables);
            });
        }
    }
}