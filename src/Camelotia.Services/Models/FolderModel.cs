using System.Collections.Generic;

namespace Camelotia.Services.Models
{
    public class FolderModel
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Children { get; set; }
    }
}
