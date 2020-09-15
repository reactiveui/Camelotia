using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class FolderView : ReactiveUserControl<IFolderViewModel>
    {
        public MenuItem TopLevelMenu => this.FindControl<MenuItem>("TopLevelMenu");

        public DrawingPresenter ArrowRight => this.FindControl<DrawingPresenter>("ArrowRight");
        public DrawingPresenter ArrowDown => this.FindControl<DrawingPresenter>("ArrowDown");

        public FolderView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {   
                this.WhenAnyValue(x => x.TopLevelMenu.IsSubMenuOpen)
                    .Do(isOpen =>
                    {
                        ArrowRight.IsVisible = !isOpen;
                        ArrowDown.IsVisible = isOpen;
                    })
                    .Subscribe()
                    .DisposeWith(disposables);                
            });
        }
    }
}