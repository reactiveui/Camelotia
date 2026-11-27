// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Runs the create-folder form for one provider.</summary>
[System.Diagnostics.DebuggerDisplay("CreateFolderViewModel: {Name}")]
public sealed partial class CreateFolderViewModel : ReactiveValidationObject, ICreateFolderViewModel
{
    /// <summary>Holds the subscriptions that keep the form and its saved state in step.</summary>
    private readonly MultipleDisposable _subscriptions = new();

    /// <summary>Backs <see cref="ErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _errorMessage;

    /// <summary>Backs <see cref="HasErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;

    /// <summary>Backs <see cref="IsLoading"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isLoading;

    /// <summary>Backs <see cref="Path"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _path;

    /// <summary>Initializes a new instance of the <see cref="CreateFolderViewModel"/> class.</summary>
    /// <param name="state">The saved form state.</param>
    /// <param name="owner">The provider view model whose current folder the new folder goes in.</param>
    /// <param name="provider">The provider that creates the folder.</param>
    public CreateFolderViewModel(CreateFolderState state, ICloudViewModel owner, ICloud provider)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(provider);

        _path = owner
            .WhenAnyValue(static x => x.CurrentPath)
            .ToProperty(this, static x => x.Path);

        _ = this.ValidationRule(
            static x => x.Name,
            static name => !string.IsNullOrWhiteSpace(name),
            "Folder name shouldn't be empty.");

        var pathRule = this.ValidationRule(
            static x => x.Path,
            static path => !string.IsNullOrWhiteSpace(path),
            "Path shouldn't be empty");

        Create = ReactiveCommand.CreateFromTask(
            cancellationToken => provider.CreateFolder(Path, Name, cancellationToken).AsTask(),
            this.IsValid());

        _isLoading = Create.IsExecuting
            .ToProperty(this, static x => x.IsLoading);

        var canInteract = owner
            .WhenAnyValue(static x => x.CanInteract)
            .Skip(1)
            .StartWith(true);

        var canOpen = this
            .WhenAnyValue(static x => x.IsVisible)
            .Select(static visible => !visible)
            .CombineLatest(
                canInteract,
                pathRule.WhenAnyValue(static x => x.IsValid),
                (closed, interact, path) => closed && provider.CanCreateFolder && interact && path);

        var canClose = this.WhenAnyValue(static x => x.IsVisible);

        Open = ReactiveCommand.Create(static () => { }, canOpen);
        Close = ReactiveCommand.Create(static () => { }, canClose);

        _subscriptions.Add(Open.Select(static _ => true)
            .Merge(Close.Select(static _ => false))
            .Subscribe(visible => IsVisible = visible));
        _subscriptions.Add(Close.Subscribe(_ => Name = string.Empty));
        _subscriptions.Add(Create.InvokeCommand(Close));

        _hasErrorMessage = Create
            .ThrownExceptions
            .Select(static _ => true)
            .Merge(Close.Select(static _ => false))
            .ToProperty(this, static x => x.HasErrorMessage);

        _errorMessage = Create
            .ThrownExceptions
            .Select(static exception => exception.Message)
            .Log(this, $"Create folder error occured in {provider.Name}")
            .Merge(Close.Select(static _ => string.Empty))
            .ToProperty(this, static x => x.ErrorMessage);

        Name = state.Name;
        IsVisible = state.IsVisible;

        _subscriptions.Add(this.WhenAnyValue(static x => x.Name).Subscribe(name => state.Name = name));
        _subscriptions.Add(this.WhenAnyValue(static x => x.IsVisible).Subscribe(visible => state.IsVisible = visible));
    }

    /// <inheritdoc/>
    [Reactive]
    public partial string Name { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial bool IsVisible { get; set; }

    /// <inheritdoc/>
    public string ErrorMessage => _errorMessage.Value;

    /// <inheritdoc/>
    public bool HasErrorMessage => _hasErrorMessage.Value;

    /// <inheritdoc/>
    public bool IsLoading => _isLoading.Value;

    /// <inheritdoc/>
    public string Path => _path.Value;

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Create { get; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Close { get; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Open { get; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscriptions.Dispose();
            _path.Dispose();
            _errorMessage.Dispose();
            _hasErrorMessage.Dispose();
            _isLoading.Dispose();
            Create.Dispose();
            Open.Dispose();
            Close.Dispose();
        }

        base.Dispose(disposing);
    }
}
