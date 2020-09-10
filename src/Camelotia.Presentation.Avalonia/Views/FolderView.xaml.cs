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

        public DrawingPresenter ArrowDrawingRight => this.FindControl<DrawingPresenter>("ArrowDrawingRight");
        public DrawingPresenter ArrowDrawingDown => this.FindControl<DrawingPresenter>("ArrowDrawingDown");


        public FolderView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {   
                this.WhenAnyValue(x => x.TopLevelMenu.IsSubMenuOpen)
                    .Do(isOpen =>
                    {
                        ArrowDrawingRight.IsVisible = !isOpen;
                        ArrowDrawingDown.IsVisible = isOpen;
                    })
                    .Subscribe()
                    .DisposeWith(disposables);                
            });
        }
    }
}