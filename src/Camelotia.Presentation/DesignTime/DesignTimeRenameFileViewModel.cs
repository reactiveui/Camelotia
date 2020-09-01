using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeRenameFileViewModel : ReactiveValidationObject<DesignTimeRenameFileViewModel>, IRenameFileViewModel
    {
        public DesignTimeRenameFileViewModel() => this.ValidationRule(x => x.NewName, name => false, "Validation error.");
        
        public bool IsLoading { get; }

        public bool IsVisible { get; set; } = false;

        public string OldName { get; } = "file";
        
        public string NewName { get; set; }

        public bool HasErrorMessage { get; } = true;

        public string ErrorMessage { get; } = "Error message example.";
        
        public ICommand Rename { get; }
        
        public ICommand Close { get; }
        
        public ICommand Open { get; }
    }
}