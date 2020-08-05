using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IAuthViewModel AuthViewModelFactory(IProvider provider);

    public sealed class AuthViewModel : ReactiveObject, IAuthViewModel
    {
        private readonly IProvider _provider;
        
        public AuthViewModel(
            IDirectAuthViewModel direct,
            IHostAuthViewModel host,
            IOAuthViewModel open,
            IProvider provider)
        {
            OAuth = open;
            HostAuth = host;
            DirectAuth = direct;
            _provider = provider;
            _provider.IsAuthorized
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Log(this, $"Authentication state changed for {provider.Name}")
                .ToPropertyEx(this, x => x.IsAuthenticated);

            this.WhenAnyValue(x => x.IsAuthenticated)
                .Select(authenticated => !authenticated)
                .ToPropertyEx(this, x => x.IsAnonymous);
        }

        public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

        public bool SupportsHostAuth => _provider.SupportsHostAuth;

        public bool SupportsOAuth => _provider.SupportsOAuth;

        [ObservableAsProperty]
        public bool IsAuthenticated { get; }

        [ObservableAsProperty]
        public bool IsAnonymous { get; }

        public IDirectAuthViewModel DirectAuth { get; }
        
        public IHostAuthViewModel HostAuth { get; }
        
        public IOAuthViewModel OAuth { get; }
    }
}