using ReactiveUI.Primitives.Reactive.Disposables;

namespace Camelotia.Presentation.Infrastructure;

public static class DisposableExtensions
{
    public static T DisposeWith<T>(this T disposable, ContainerDisposable container)
        where T : IDisposable
    {
        container.Add(disposable);
        return disposable;
    }
}
