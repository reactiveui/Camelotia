using Camelotia.Presentation.Interfaces;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainProviderView : ReactiveViewCell<IProviderViewModel>
    {
        public MainProviderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel, x => x.Name, x => x.NameLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Description, x => x.DescriptionLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Size, x => x.SizeLabel.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}