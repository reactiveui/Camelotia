using System;

namespace Camelotia.Services.Models
{
    public class ProviderModel
    {
        public Guid Id { get; set; }
        
        public string Type { get; set; }
        
        public string Token { get; set; }
        
        public string User { get; set; }
    }
}