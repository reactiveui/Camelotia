// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Text.Json;
using Akavache;
using Akavache.SystemTextJson;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Services.Models;
using Camelotia.Tests.Fakes;
using DynamicData;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests the suspension driver, the app state serialization, the composition host and the disposable helper.</summary>
public sealed class InfrastructureTests
{
    /// <summary>Saves, loads and removes the app state.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task AkavacheSuspensionDriver_SavesLoadsAndInvalidates()
    {
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        var driver = new AkavacheSuspensionDriver(cache);
        using var state = new MainState { SelectedSupportedType = CloudType.Ftp };
        state.Clouds.AddOrUpdate(new CloudState { Type = CloudType.Sftp, CurrentPath = "/home" });

        // A missing state fails the load, which tells ReactiveUI to create a new state.
        _ = await Assert.That(async () => await driver.LoadState(AppStateJsonContext.Default.MainState).ToTask()).Throws<KeyNotFoundException>();
        _ = await driver.SaveState(state, AppStateJsonContext.Default.MainState).ToTask();
        using var loaded = await driver.LoadState(AppStateJsonContext.Default.MainState).ToTask();
        _ = await driver.InvalidateState().ToTask();

        await Assert.That(loaded!.SelectedSupportedType).IsEqualTo(CloudType.Ftp);
        await Assert.That(loaded.Clouds.Items[0].CurrentPath).IsEqualTo("/home");
        _ = await Assert.That(async () => await driver.LoadState(AppStateJsonContext.Default.MainState).ToTask()).Throws<KeyNotFoundException>();
    }

    /// <summary>Loads no state when the cache holds an empty entry.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task AkavacheSuspensionDriver_LoadsNothingFromAnEmptyEntry()
    {
        var cache = new Infrastructure.InterfaceFake<IBlobCache>().On(nameof(IBlobCache.Get), static _ => Signal.Emit<byte[]?>(null));
        var driver = new AkavacheSuspensionDriver(cache.Instance);

        var loaded = await driver.LoadState(AppStateJsonContext.Default.MainState).ToTask();

        await Assert.That(loaded).IsNull();
    }

    /// <summary>Serializes every saved field with the generated metadata.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task AppStateJsonContext_RoundTripsTheState()
    {
        using var state = new MainState { SelectedProviderId = Guid.NewGuid() };
        var cloud = new CloudState { Token = "t", User = "u", Created = Infrastructure.TestValues.Moment };
        cloud.CreateFolderState.Name = "folder";
        cloud.RenameFileState.NewName = "name";
        cloud.AuthState.DirectAuthState.Username = "direct";
        cloud.AuthState.HostAuthState.Address = "host";
        state.Clouds.AddOrUpdate(cloud);

        var json = JsonSerializer.Serialize(state, AppStateJsonContext.Default.MainState);
        using var copy = JsonSerializer.Deserialize(json, AppStateJsonContext.Default.MainState)!;
        var restored = copy.Clouds.Items[0];

        await Assert.That(copy.SelectedProviderId).IsEqualTo(state.SelectedProviderId);
        await Assert.That((restored.Token, restored.User, restored.Created)).IsEqualTo(("t", "u", Infrastructure.TestValues.Moment));
        await Assert.That(restored.CreateFolderState.Name).IsEqualTo("folder");
        await Assert.That(restored.RenameFileState.NewName).IsEqualTo("name");
        await Assert.That(restored.AuthState.DirectAuthState.Username).IsEqualTo("direct");
        await Assert.That(restored.AuthState.HostAuthState.Address).IsEqualTo("host");
        await Assert.That(restored.Parameters.Id).IsEqualTo(cloud.Id);
    }

    /// <summary>Builds the main view model over the real services, and disposes them.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task CamelotiaHost_BuildsTheMainViewModel()
    {
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        using var state = new MainState();
        state.Clouds.AddOrUpdate(new CloudState { Type = CloudType.Local });

        using (var host = new CamelotiaHost(state, cache, new FakeFileManager()))
        {
            await Assert.That(host.MainViewModel.Clouds.Count).IsEqualTo(1);
            await Assert.That(host.MainViewModel.Clouds[0].Name).IsEqualTo("Local");
            await Assert.That(host.MainViewModel.Clouds[0].Folder).IsNotNull();
        }

        _ = await Assert.That(() => new CamelotiaHost(null!, cache, new FakeFileManager())).Throws<ArgumentNullException>();
    }
}
