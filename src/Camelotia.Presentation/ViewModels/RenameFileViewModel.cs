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

/// <summary>Runs the rename form for the selected file of one provider.</summary>
[System.Diagnostics.DebuggerDisplay("RenameFileViewModel: {OldName}")]
public sealed partial class RenameFileViewModel : ReactiveValidationObject, IRenameFileViewModel
{
    /// <summary>Holds the subscriptions that keep the form and its saved state in step.</summary>
    private readonly MultipleDisposable _subscriptions = new();

    /// <summary>Backs <see cref="HasErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;

    /// <summary>Backs <see cref="ErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _errorMessage;

    /// <summary>Backs <see cref="IsLoading"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isLoading;

    /// <summary>Backs <see cref="OldName"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _oldName;

    /// <summary>Initializes a new instance of the <see cref="RenameFileViewModel"/> class.</summary>
    /// <param name="state">The saved form state.</param>
    /// <param name="owner">The provider view model whose selected file the form renames.</param>
    /// <param name="provider">The provider that renames the file.</param>
    public RenameFileViewModel(RenameFileState state, ICloudViewModel owner, ICloud provider)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(provider);

        _oldName = owner
            .WhenAnyValue(static x => x.SelectedFile)
            .Select(static file => file?.Name ?? string.Empty)
            .ToProperty(this, static x => x.OldName, string.Empty);

        _ = this.ValidationRule(
            static x => x.NewName,
            static name => !string.IsNullOrWhiteSpace(name),
            "New name shouldn't be empty.");

        var oldRule = this.ValidationRule(
            static x => x.OldName,
            static name => !string.IsNullOrWhiteSpace(name),
            "Old name shouldn't be empty.");

        Rename = ReactiveCommand.CreateFromTask(
            cancellationToken =>
            {
                var file = owner.SelectedFile ?? throw new InvalidOperationException("Select a file to rename.");
                return provider.RenameFile(file.Path, NewName, cancellationToken).AsTask();
            },
            this.IsValid());

        _isLoading = Rename
            .IsExecuting
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
                oldRule.WhenAnyValue(static x => x.IsValid),
                static (closed, interact, old) => closed && old && interact);

        var canClose = this.WhenAnyValue(static x => x.IsVisible);

        Open = ReactiveCommand.Create(static () => { }, canOpen);
        Close = ReactiveCommand.Create(static () => { }, canClose);

        _subscriptions.Add(Open.Select(static _ => true)
            .Merge(Close.Select(static _ => false))
            .Subscribe(visible => IsVisible = visible));
        _subscriptions.Add(Close.Subscribe(_ => NewName = string.Empty));
        _subscriptions.Add(Rename.InvokeCommand(Close));

        _hasErrorMessage = Rename
            .ThrownExceptions
            .Select(static _ => true)
            .Merge(Close.Select(static _ => false))
            .ToProperty(this, static x => x.HasErrorMessage);

        _errorMessage = Rename
            .ThrownExceptions
            .Select(static exception => exception.Message)
            .Log(this, $"Rename file error occured in {provider.Name}")
            .Merge(Close.Select(static _ => string.Empty))
            .ToProperty(this, static x => x.ErrorMessage);

        NewName = state.NewName;
        _subscriptions.Add(this.WhenAnyValue(static x => x.NewName).Subscribe(name => state.NewName = name));
    }

    /// <inheritdoc/>
    [Reactive]
    public partial bool IsVisible { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial string NewName { get; set; }

    /// <inheritdoc/>
    public bool HasErrorMessage => _hasErrorMessage.Value;

    /// <inheritdoc/>
    public string ErrorMessage => _errorMessage.Value;

    /// <inheritdoc/>
    public bool IsLoading => _isLoading.Value;

    /// <inheritdoc/>
    public string OldName => _oldName.Value;

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Rename { get; }

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
            _oldName.Dispose();
            _hasErrorMessage.Dispose();
            _errorMessage.Dispose();
            _isLoading.Dispose();
            Rename.Dispose();
            Open.Dispose();
            Close.Dispose();
        }

        base.Dispose(disposing);
    }
}
