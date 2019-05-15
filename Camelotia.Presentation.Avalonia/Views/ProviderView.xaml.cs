using System;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class ProviderView : ReactiveUserControl<IProviderViewModel>
    {
        public ProviderView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }

        public void OnPointerReleased(object sender, PointerReleasedEventArgs args)
        {
            Console.WriteLine("Hello");
        }
    }
}