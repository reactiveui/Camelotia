using System;
using System.Reactive;
using Akavache;
using ReactiveUI;

namespace Camelotia.Presentation.Infrastructure
{
    public class AkavacheSuspensionDriver<TAppState> : ISuspensionDriver where TAppState : class
    {
        private const string Key = "camelotia-state";
  
        public AkavacheSuspensionDriver() => BlobCache.ApplicationName = "Camelotia";

        public IObservable<Unit> InvalidateState() => BlobCache.UserAccount.InvalidateObject<TAppState>(Key);
  
        public IObservable<object> LoadState() => BlobCache.UserAccount.GetObject<TAppState>(Key);

        public IObservable<Unit> SaveState(object state) => BlobCache.UserAccount.InsertObject(Key, (TAppState)state);
    }
}