namespace Camelotia.Services.Models;

public class FolderModel
{
    public FolderModel(string fullPath, string name, IEnumerable<FolderModel> children = null)
    {
        FullPath = fullPath;
        Name = name;
        Children = children ?? Enumerable.Empty<FolderModel>();
    }

    public string FullPath { get; }

    public string Name { get; }

    public IEnumerable<FolderModel> Children { get; }
}