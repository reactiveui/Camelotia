namespace Camelotia.Services.Models
{
    public sealed class FileModel
    {
        public FileModel(string name, string path, bool isFolder, string size)
        {
            Name = name;
            Path = path;
            IsFolder = isFolder;
            Size = size;
        }
        
        public string Name { get; }

        public string Path { get; }

        public bool IsFolder { get; }

        public string Size { get; }
    }
}