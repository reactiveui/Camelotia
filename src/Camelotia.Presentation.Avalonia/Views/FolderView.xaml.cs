using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed partial class FolderView : ReactiveUserControl<IFolderViewModel>
    {
        public FolderView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.TopLevelMenu.IsSubMenuOpen)
                    .BindTo(this, x => x.ArrowDown.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.TopLevelMenu.IsSubMenuOpen)
                    .Select(menuOpen => !menuOpen)
                    .BindTo(this, x => x.ArrowRight.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}
