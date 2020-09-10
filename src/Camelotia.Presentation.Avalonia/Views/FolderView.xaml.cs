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

        public DrawingPresenter ArrowDrawing => this.FindControl<DrawingPresenter>("ArrowDrawing");

        public FolderView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.TopLevelMenu.IsSubMenuOpen)                    
                    .Select(open => open ? (GeometryDrawing)this.FindResource("ChevronDown") : (GeometryDrawing)this.FindResource("ChevronRight"))
                    .Do(d => ArrowDrawing.Drawing = d)
                    .Subscribe()
                    .DisposeWith(disposables);                
            });
        }
    }
}