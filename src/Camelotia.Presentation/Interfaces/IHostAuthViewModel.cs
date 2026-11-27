// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Presentation.Interfaces;

/// <summary>The host, port, user name and password sign-in form.</summary>
public interface IHostAuthViewModel : IDirectAuthViewModel
{
    /// <summary>Gets or sets the server address.</summary>
    string Address { get; set; }

    /// <summary>Gets or sets the server port, as typed.</summary>
    string Port { get; set; }
}
