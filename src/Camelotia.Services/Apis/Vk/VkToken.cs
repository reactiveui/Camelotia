// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>A VK ID token reply.</summary>
/// <param name="AccessToken">The access token.</param>
/// <param name="RefreshToken">The token that renews the access token.</param>
/// <param name="UserId">The signed-in user's identifier.</param>
/// <param name="State">The state value sent with the request.</param>
[System.Diagnostics.DebuggerDisplay("VkToken: {AccessToken}")]
public sealed record VkToken(string AccessToken, string? RefreshToken, long UserId, string? State);
