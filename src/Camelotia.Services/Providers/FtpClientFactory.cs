// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using FluentFTP;

namespace Camelotia.Services.Providers;

/// <summary>Creates an unconnected FTP client for a server.</summary>
/// <param name="host">The server address.</param>
/// <param name="port">The server port.</param>
/// <param name="user">The user name.</param>
/// <param name="password">The password.</param>
/// <returns>The client, which the caller connects and disposes.</returns>
public delegate IAsyncFtpClient FtpClientFactory(string host, int port, string user, string password);
