using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services;

public sealed class AvaloniaFileManager : IFileManager
{
    private readonly Window _window;

    public AvaloniaFileManager(Window window) => _window = window;

    public async Task<Stream> OpenWrite(string name)
    {
        var folders = await _window.StorageProvider
            .OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false })
            .ConfigureAwait(false);
        var path = Path.Combine(folders[0].Path.LocalPath, name);
        return File.Create(path);
    }

    public async Task<(string Name, Stream Stream)> OpenRead()
    {
        var files = await _window.StorageProvider
            .OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false })
            .ConfigureAwait(false);
        var file = files[0];
        var stream = await file.OpenReadAsync().ConfigureAwait(false);
        return (file.Name, stream);
    }
}
