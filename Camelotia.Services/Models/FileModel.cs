namespace Camelotia.Services.Models
{
    public sealed class FileModel
    {
        public string Name { get; }

        public string Path { get; }

        public bool IsFolder { get; }

        public bool IsFile => !IsFolder;

        public string Size { get; }
        
        public FileModel(string name, string path, bool isFolder, string size)
        {
            Name = name;
            Path = path;
            IsFolder = isFolder;
            Size = size;
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