using System;
using System.Runtime.Serialization;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class ProviderState
    {
        [DataMember]
        public CreateFolderState CreateFolderState { get; set; } = new CreateFolderState();
        
        [DataMember]
        public RenameFileState RenameFileState { get; set; } = new RenameFileState();
        
        [DataMember]
        public AuthState AuthState { get; set; } = new AuthState();
        
        [DataMember]
        public string CurrentPath { get; set; }
        
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [DataMember]
        public DateTime Created { get; set; } = DateTime.Now;
        
        [DataMember]
        public ProviderType Type { get; set; } = ProviderType.Local;
        
        [DataMember]
        public string User { get; set; }
        
        [DataMember]
        public string Token { get; set; }
        
        [IgnoreDataMember]
        public ProviderParameters Parameters => new ProviderParameters
        {
            Id = Id,
            Created = Created,
            Type = Type,
            User = User,
            Token = Token
        };
    }
}