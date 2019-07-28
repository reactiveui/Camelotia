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
    public partial class MainView : ReactiveNavigationPage<IMainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel)
                    .Select(x => new MainMasterView {ViewModel = x})
                    .Subscribe(view => PushAsync(view))
                    .DisposeWith(disposables);
            });
        }
    }
}