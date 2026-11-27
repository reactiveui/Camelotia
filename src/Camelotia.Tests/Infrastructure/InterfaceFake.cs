// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Reflection;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Implements a large library interface for a test, answering only the methods the test configures.</summary>
/// <typeparam name="T">The interface to implement.</typeparam>
/// <remarks>Library interfaces such as FluentFTP's <c>IAsyncFtpClient</c> have hundreds of members; a hand-written fake would be mostly empty stubs.</remarks>
[System.Diagnostics.DebuggerDisplay("InterfaceFake: {typeof(T).Name}")]
public sealed class InterfaceFake<T>
    where T : class
{
    /// <summary>The answer for each configured method name.</summary>
    private readonly Dictionary<string, Func<object?[], object?>> _handlers = [with(StringComparer.Ordinal)];

    /// <summary>Gets the fake implementation to pass to the code under test. It is created on first read.</summary>
    public T Instance => field ??= CreateProxy();

    /// <summary>Gets the names of the methods called, in order.</summary>
    public List<string> Calls { get; } = [];

    /// <summary>Gets the arguments of each call, in order.</summary>
    public List<object?[]> Arguments { get; } = [];

    /// <summary>Sets the answer for every overload of a method.</summary>
    /// <param name="methodName">The method name.</param>
    /// <param name="handler">Returns the result from the call's arguments.</param>
    /// <returns>This fake, for chaining.</returns>
    public InterfaceFake<T> On(string methodName, Func<object?[], object?> handler)
    {
        _handlers[methodName] = handler;
        return this;
    }

    /// <summary>Creates the proxy that forwards calls to this fake.</summary>
    /// <returns>The proxy.</returns>
    private T CreateProxy()
    {
        var proxy = DispatchProxy.Create<T, Dispatcher>();
        ((Dispatcher)(object)proxy).Owner = this;
        return proxy;
    }

    /// <summary>Answers a call through the configured handler, or with a completed default for an async method.</summary>
    /// <param name="method">The method called.</param>
    /// <param name="args">The call arguments.</param>
    /// <returns>The result.</returns>
    /// <exception cref="NotSupportedException">The test did not configure the method.</exception>
    private object? Answer(MethodInfo method, object?[] args)
    {
        Calls.Add(method.Name);
        Arguments.Add(args);
        if (_handlers.TryGetValue(method.Name, out var handler))
        {
            return handler(args);
        }

        if (method.Name is "Dispose")
        {
            return null;
        }

        if (method.Name is "DisposeAsync" or "Connect" or "ConnectAsync" || method.ReturnType == typeof(Task))
        {
            return method.ReturnType == typeof(ValueTask) ? ValueTask.CompletedTask : Task.CompletedTask;
        }

        throw new NotSupportedException($"The test did not configure {typeof(T).Name}.{method.Name}.");
    }

    /// <summary>Forwards each interface call to the owning fake.</summary>
    [System.Diagnostics.DebuggerDisplay("Dispatcher")]
    public class Dispatcher : DispatchProxy
    {
        /// <summary>Gets or sets the fake that answers the calls.</summary>
        internal InterfaceFake<T>? Owner { get; set; }

        /// <inheritdoc/>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            Owner!.Answer(targetMethod!, args ?? []);
    }
}
