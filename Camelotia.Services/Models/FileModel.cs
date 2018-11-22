namespace Camelotia.Services.Models
{
    public sealed class FileModel
    {
        public FileModel(string name, string path, bool isFolder, string size)
        {
            Name = name;
            Path = path;
            Size = size;
            IsFolder = isFolder;
        }
        
        public bool IsFolder { get; }
        
        public string Name { get; }
        
        public string Path { get; }
        
        public string Size { get; }
    }
}