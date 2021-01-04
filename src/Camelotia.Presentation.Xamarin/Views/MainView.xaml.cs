using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ReactiveNavigationPage<IMainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel)
                    .Select(x => new MainMasterView { ViewModel = x })
                    .Subscribe(view => PushAsync(view))
                    .DisposeWith(disposables);
            });
        }
    }
}