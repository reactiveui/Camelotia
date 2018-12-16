namespace Camelotia.Services.Models
{
    public sealed class FileModel
    {
        public FileModel(string name, string path, bool isFolder, bool isDrive, string size)
        {
            Name = name;
            Path = path;
            IsFolder = isFolder;
            IsDrive = isDrive;
            Size = size;
        }
        public string Name { get; }

        public string Path { get; }

        public bool IsFolder { get; }

        public bool IsDrive { get; }
       
        public string Size { get; }
    }
}