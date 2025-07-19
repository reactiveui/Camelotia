using System.Reactive;
using Akavache;
using Newtonsoft.Json;
using ReactiveUI;
using Splat;

namespace Camelotia.Presentation.Infrastructure;

public class AkavacheSuspensionDriver<TAppState> : ISuspensionDriver
    where TAppState : class
{
    private const string Key = "camelotia-state";

    static AkavacheSuspensionDriver() => Locator.CurrentMutable.RegisterConstant(new JsonSerializerSettings
    {
        ObjectCreationHandling = ObjectCreationHandling.Replace
    });

    public AkavacheSuspensionDriver(string appName = "CamelotiaV2") => BlobCache.ApplicationName = appName;

    public IObservable<Unit> InvalidateState() => BlobCache.UserAccount.InvalidateObject<TAppState>(Key);

    public IObservable<object> LoadState() => BlobCache.UserAccount.GetObject<TAppState>(Key);

    public IObservable<Unit> SaveState(object state) => BlobCache.UserAccount.InsertObject(Key, (TAppState)state);
}