using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Serialization.Metadata;
using Akavache.Reactive;
using ReactiveUI;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Infrastructure;

public class AkavacheSuspensionDriver<TAppState> : ISuspensionDriver
    where TAppState : class
{
    private const string Key = "camelotia-state";

    public IObservable<Unit> InvalidateState() => CacheDatabase.UserAccount.InvalidateObject<TAppState>(Key);

    public IObservable<object?> LoadState() => CacheDatabase.UserAccount.GetObject<TAppState>(Key).Select(static x => (object?)x);

    public IObservable<T?> LoadState<T>(JsonTypeInfo<T> typeInfo) => CacheDatabase.UserAccount.GetObject<T>(Key).Select(static x => (T?)x);

    public IObservable<Unit> SaveState<T>(T state) => CacheDatabase.UserAccount.InsertObject(Key, state);

    public IObservable<Unit> SaveState<T>(T state, JsonTypeInfo<T> typeInfo) => CacheDatabase.UserAccount.InsertObject(Key, state);
}