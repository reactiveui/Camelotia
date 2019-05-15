using System;

namespace Camelotia.Services.Models
{
    public sealed class FileModel
    {
        public string Name { get; }

        public string Path { get; }

        public bool IsFolder { get; }

        public bool IsFile => !IsFolder;

        public string Size { get; }

        public string Modified { get; }
        
        public FileModel(string name, string path, bool isFolder, string size, DateTime? modified = null)
        {
            Path = path;
            Size = size;
            IsFolder = isFolder;
            Modified = modified?.ToString();
            Name = name;
        }
    }
}