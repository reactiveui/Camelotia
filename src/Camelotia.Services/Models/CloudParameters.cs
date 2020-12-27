using System;

namespace Camelotia.Services.Models
{
    public class CloudParameters
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Created { get; set; } = DateTime.Now;
        public CloudType Type { get; set; } = CloudType.Local;
        public string User { get; set; }
        public string Token { get; set; }
    }
}