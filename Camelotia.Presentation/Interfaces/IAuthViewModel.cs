using System.ComponentModel;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Interfaces
{
    public delegate IAuthViewModel AuthViewModelFactory(IProvider provider);

    public interface IAuthViewModel : INotifyPropertyChanged
    {
        IDirectAuthViewModel DirectAuth { get; }
        
        IHostAuthViewModel HostAuth { get; }
        
        IOAuthViewModel OAuth { get; }
        
        bool SupportsDirectAuth { get; }
        
        bool SupportsHostAuth { get; }
        
        bool SupportsOAuth { get; }
        
        bool IsAuthenticated { get; }

        bool IsAnonymous { get; }
    }
}