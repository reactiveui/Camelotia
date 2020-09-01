using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeDirectAuthViewModel : ReactiveValidationObject<DesignTimeDirectAuthViewModel>, IDirectAuthViewModel
    {
        public DesignTimeDirectAuthViewModel() => this.ValidationRule(x => x.Username, name => false, "Validation error.");
        
        public string Username { get; set; } = "Joseph";
        
        public string Password { get; set; }
        
        public ICommand Login { get; }

        public bool HasErrorMessage { get; } = true;

        public string ErrorMessage { get; } = "Error message example.";
        
        public bool IsBusy { get; }
    }
}