using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class FolderView : ReactiveUserControl<IFolderViewModel>
    {
        private MenuItem TopLevelMenu => this.FindControl<MenuItem>("TopLevelMenu");

        private DrawingPresenter ArrowRight => this.FindControl<DrawingPresenter>("ArrowRight");

        private DrawingPresenter ArrowDown => this.FindControl<DrawingPresenter>("ArrowDown");

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