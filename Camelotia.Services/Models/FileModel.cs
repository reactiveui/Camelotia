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
            Name = name;
            Path = path;
            Size = size;
            IsFolder = isFolder;
            Modified = modified?.ToString();
        }

        public override int GetHashCode() => (Name, Path, IsFolder, Size).GetHashCode();

        public override bool Equals(object obj)
        {
            var file = obj as FileModel;
            return 
                file != null &&
                file.Name == Name &&
                file.Path == Path &&
                file.IsFolder == IsFolder &&
                file.Size == Size;
        }
    }
}