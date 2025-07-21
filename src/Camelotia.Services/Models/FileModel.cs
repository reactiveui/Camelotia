namespace Camelotia.Services.Models;

public class FileModel
{
    public string Name { get; set; }

    public string Path { get; set; }

    public long Size { get; set; }

    public bool IsFolder { get; set; }

    public DateTime? Modified { get; set; }
}