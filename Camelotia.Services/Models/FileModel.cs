namespace Camelotia.Services.Models
{
    public struct FileModel
    {
        public string Name { get; }

        public string Path { get; }

        public bool IsFolder { get; }

        public string Size { get; }
        
        public FileModel(string name, string path, bool isFolder, string size)
        {
            Name = name;
            Path = path;
            IsFolder = isFolder;
            Size = size;
        }
    }
}