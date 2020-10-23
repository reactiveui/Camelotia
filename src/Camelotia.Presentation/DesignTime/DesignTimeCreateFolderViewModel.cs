using System.Reactive;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeCreateFolderViewModel : ReactiveValidationObject, ICreateFolderViewModel
    {
        public DesignTimeCreateFolderViewModel() => this.ValidationRule(x => x.Name, name => false, "Validation error.");
        
        public bool IsLoading { get; }

        public bool IsVisible { get; set; } = false;

        public string Name { get; set; }

        public string Path { get; } = "/home/path";

        public bool HasErrorMessage { get; } = true;

        public string ErrorMessage { get; } = "Error message example.";
        
        public ReactiveCommand<Unit, Unit> Create { get; }
        
        public ReactiveCommand<Unit, Unit> Close { get; }
        
        public ReactiveCommand<Unit, Unit> Open { get; }
    }
}