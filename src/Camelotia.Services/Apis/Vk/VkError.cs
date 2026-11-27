// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>A failure VK reports inside a reply.</summary>
/// <param name="ErrorCode">The VK error code.</param>
/// <param name="ErrorMsg">The VK error message.</param>
[System.Diagnostics.DebuggerDisplay("VkError: {ErrorCode}")]
public sealed record VkError(int ErrorCode, string ErrorMsg);
