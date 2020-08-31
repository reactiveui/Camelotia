using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeOAuthViewModel : ReactiveObject, IOAuthViewModel
    {
        public ICommand Login { get; }
        
        public bool HasErrorMessage { get; } = true;
        
        public string ErrorMessage { get; } = "Error message example.";
        
        public bool IsBusy { get; }
    }
}