using System;
using System.Reactive;
using Avalonia;
using ReactiveUI.Avalonia.Reactive;

namespace Camelotia.Presentation.Avalonia;

public static class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseReactiveUI(static builder => builder.WithExceptionHandler(Observer.Create<Exception>(Console.WriteLine)))
            .UsePlatformDetect()
            .LogToTrace();
}