using System.Runtime.Serialization;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class ProviderState : ProviderModel
    {
        public CreateFolderState CreateFolderState { get; set; } = new CreateFolderState();
        public RenameFileState RenameFileState { get; set; } = new RenameFileState();
        public AuthState AuthState { get; set; } = new AuthState();
    }
}