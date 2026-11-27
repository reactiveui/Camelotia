// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Akavache;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Camelotia.Presentation.Infrastructure;

/// <summary>Builds the services and the main view model that both desktop apps share.</summary>
[System.Diagnostics.DebuggerDisplay("CamelotiaHost")]
public sealed class CamelotiaHost : IDisposable
{
    /// <summary>The container that owns the Refit clients and their HTTP handlers.</summary>
    private readonly ServiceProvider _services;

    /// <summary>Backs <see cref="MainViewModel"/>.</summary>
    private readonly MainViewModel _main;

    /// <summary>Initializes a new instance of the <see cref="CamelotiaHost"/> class.</summary>
    /// <param name="state">The saved app state.</param>
    /// <param name="cache">The cache that stores provider credentials.</param>
    /// <param name="files">Asks the user for local files, using the platform's dialogs.</param>
    public CamelotiaHost(MainState state, IBlobCache cache, IFileManager files)
    {
        ArgumentNullException.ThrowIfNull(state);
        _services = new ServiceCollection()
            .AddSingleton(state.CloudConfiguration)
            .AddSingleton(cache)
            .AddCloudServices()
            .BuildServiceProvider();

        _main = new(
            state,
            _services.GetRequiredService<ICloudFactory>(),
            (cloudState, provider) => CreateCloudViewModel(cloudState, provider, files),
            TimeProvider.System);
    }

    /// <summary>Gets the main view model.</summary>
    public MainViewModel MainViewModel => _main;

    /// <inheritdoc/>
    public void Dispose()
    {
        _main.Dispose();
        _services.Dispose();
    }

    /// <summary>Creates the browser view model, with its forms, for one provider.</summary>
    /// <param name="state">The provider's saved state.</param>
    /// <param name="provider">The provider.</param>
    /// <param name="files">Asks the user for local files.</param>
    /// <returns>The browser view model.</returns>
    private static CloudViewModel CreateCloudViewModel(CloudState state, ICloud provider, IFileManager files)
    {
        var factories = new CloudViewModelFactories(
            owner => new CreateFolderViewModel(state.CreateFolderState, owner, provider),
            owner => new RenameFileViewModel(state.RenameFileState, owner, provider),
            static (file, owner) => new FileViewModel(owner, file),
            static (folder, owner) => new FolderViewModel(owner, folder));
        var auth = new AuthViewModel(
            new DirectAuthViewModel(state.AuthState.DirectAuthState, provider),
            new HostAuthViewModel(state.AuthState.HostAuthState, provider),
            new OAuthViewModel(provider),
            provider);
        return new(state, factories, auth, files, provider);
    }
}
