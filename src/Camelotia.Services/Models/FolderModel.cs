using System.Collections.Generic;

namespace Camelotia.Services.Models
{
    public class FolderModel
    {
        public FolderModel(string fullPath, string name, IEnumerable<string> children)
        {
            FullPath = fullPath;
            Name = name;
            Children = children;
        }
        
        public string FullPath { get; }
        public string Name { get; }
        public IEnumerable<string> Children { get; }
    }
}
