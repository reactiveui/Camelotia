// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>A VK API reply. VK answers with HTTP 200 and fills <see cref="Error"/> when a method fails.</summary>
/// <typeparam name="T">The type of the result.</typeparam>
/// <param name="Response">The result, or the default value when the method failed.</param>
/// <param name="Error">The failure, or <see langword="null"/> when the method succeeded.</param>
[System.Diagnostics.DebuggerDisplay("VkReply: {Response}")]
public sealed record VkReply<T>(T? Response, VkError? Error)
{
    /// <summary>Returns the result, or throws when VK reported a failure.</summary>
    /// <returns>The result.</returns>
    /// <exception cref="VkApiException">VK reported a failure or sent no result.</exception>
    public T EnsureResult()
    {
        if (Error is { } error)
        {
            throw new VkApiException(error.ErrorCode, error.ErrorMsg);
        }

        return Response ?? throw new VkApiException(0, "VK sent an empty reply.");
    }
}
