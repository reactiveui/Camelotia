using System.ComponentModel;

namespace Camelotia.Presentation.Interfaces
{
    public interface IAuthViewModel : INotifyPropertyChanged
    {
        IDirectAuthViewModel DirectAuth { get; }
        
        IOAuthViewModel OAuth { get; }
        
        bool SupportsDirectAuth { get; }
        
        bool SupportsOAuth { get; }
        
        bool IsAuthenticated { get; }

        bool IsAnonymous { get; }
    }
}