// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Reads and runs ReactiveUI commands in tests.</summary>
public static class CommandExtensions
{
    /// <summary>Extends a command.</summary>
    /// <param name="command">The command.</param>
    extension(IReactiveCommand command)
    {
        /// <summary>Reads whether the command can run now.</summary>
        /// <returns><see langword="true"/> when the command can run.</returns>
        public bool CanRunNow()
        {
            var canExecute = false;
            using var subscription = command.CanExecute.Take(1).Subscribe(value => canExecute = value);
            return canExecute;
        }
    }

    /// <summary>Extends a command with a result.</summary>
    /// <typeparam name="TParameter">The parameter type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="command">The command.</param>
    extension<TParameter, TResult>(ReactiveCommand<TParameter, TResult> command)
    {
        /// <summary>Runs the command and waits for it to finish, swallowing a failure; read <c>ThrownExceptions</c> to see it.</summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>A task that completes when the command finishes.</returns>
        public async Task RunAsync(TParameter parameter)
        {
            try
            {
                _ = await command.Execute(parameter).ToTask().ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // The command reports the failure through ThrownExceptions, which the test reads.
            }
        }
    }
}
