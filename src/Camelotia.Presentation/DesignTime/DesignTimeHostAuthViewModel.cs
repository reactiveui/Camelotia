using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeHostAuthViewModel : ReactiveValidationObject<DesignTimeHostAuthViewModel>, IHostAuthViewModel
    {
        public DesignTimeHostAuthViewModel() => this.ValidationRule(x => x.Username, name => false, "Validation error.");
        
        public string Username { get; set; } = "Jotaro";

        public string Password { get; set; } = "Qwerty";
        
        public ICommand Login { get; }

        public bool HasErrorMessage { get; } = true;

        public string ErrorMessage { get; } = "Error message example.";
        
        public bool IsBusy { get; }

        public string Address { get; set; } = "127.0.0.1";

        public string Port { get; set; } = "5001";
    }
}