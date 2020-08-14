using System;

namespace Camelotia.Services.Models
{
    public class ProviderModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Created { get; set; } = DateTime.Now;
        public ProviderType Type { get; set; }
        public string User { get; set; }
        public string Token { get; set; }
    }
}