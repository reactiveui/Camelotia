// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Tests.Fakes;

/// <summary>An <see cref="ICloud"/> whose answers each test sets, and which records the calls it receives.</summary>
[System.Diagnostics.DebuggerDisplay("FakeCloud: {Name}")]
public sealed class FakeCloud : ICloud, IDisposable
{
    /// <summary>Backs <see cref="IsAuthorized"/>.</summary>
    private readonly BehaviorSignal<bool> _isAuthorized = new(false);

    /// <inheritdoc/>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc/>
    public long? Size { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = "Fake";

    /// <inheritdoc/>
    public DateTimeOffset Created { get; set; }

    /// <inheritdoc/>
    public string InitialPath { get; set; } = "/";

    /// <inheritdoc/>
    public bool CanCreateFolder { get; set; } = true;

    /// <inheritdoc/>
    public IObservable<bool> IsAuthorized => _isAuthorized;

    /// <inheritdoc/>
    public CloudParameters Parameters { get; set; } = new();

    /// <inheritdoc/>
    public bool SupportsDirectAuth { get; set; }

    /// <inheritdoc/>
    public bool SupportsHostAuth { get; set; }

    /// <inheritdoc/>
    public bool SupportsOAuth { get; set; }

    /// <summary>Gets or sets the listing for each path.</summary>
    public Func<string, IEnumerable<FileModel>> Files { get; set; } = static _ => [];

    /// <summary>Gets or sets the breadcrumb trail for each path.</summary>
    public Func<string, IEnumerable<FolderModel>> BreadCrumbs { get; set; } = static _ => [];

    /// <summary>Gets or sets what each write operation does; the argument names the operation.</summary>
    public Func<string, Task> Operation { get; set; } = static _ => Task.CompletedTask;

    /// <summary>Gets the operations called, such as <c>CreateFolder /a b</c>, in order.</summary>
    public List<string> Calls { get; } = [];

    /// <summary>Sets whether the user is signed in.</summary>
    /// <param name="authorized">Whether the user is signed in.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAuthorized(bool authorized) => _isAuthorized.OnNext(authorized);

    /// <inheritdoc/>
    public ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        Calls.Add($"GetFiles {path}");
        return ValueTask.FromResult(Files(path));
    }

    /// <inheritdoc/>
    public ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken)
    {
        Calls.Add($"GetBreadCrumbs {path}");
        return ValueTask.FromResult(BreadCrumbs(path));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken) => Run($"UploadFile {toPath} {name}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken) => Run($"DownloadFile {fromPath}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken) => Run($"CreateFolder {path} {name}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask RenameFile(string path, string name, CancellationToken cancellationToken) => Run($"RenameFile {path} {name}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken) => Run($"Delete {path} {isFolder}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) => Run($"DirectAuth {login} {password}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken) =>
        Run($"HostAuth {address} {port} {login} {password}");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask OAuth(CancellationToken cancellationToken) => Run("OAuth");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Logout(CancellationToken cancellationToken) => Run("Logout");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _isAuthorized.Dispose();

    /// <summary>Records an operation and runs the configured behaviour for it.</summary>
    /// <param name="call">The operation and its arguments.</param>
    /// <returns>The configured behaviour's task.</returns>
    private ValueTask Run(string call)
    {
        Calls.Add(call);
        return new(Operation(call));
    }
}
