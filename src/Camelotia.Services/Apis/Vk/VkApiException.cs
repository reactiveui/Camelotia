// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>Thrown when VK reports that a method failed.</summary>
[System.Diagnostics.DebuggerDisplay("VkApiException: {ErrorCode} {Message}")]
public sealed class VkApiException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="VkApiException"/> class.</summary>
    public VkApiException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="VkApiException"/> class.</summary>
    /// <param name="message">The failure message.</param>
    public VkApiException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="VkApiException"/> class.</summary>
    /// <param name="message">The failure message.</param>
    /// <param name="innerException">The failure that caused this one.</param>
    public VkApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="VkApiException"/> class.</summary>
    /// <param name="errorCode">The VK error code.</param>
    /// <param name="message">The VK error message.</param>
    public VkApiException(int errorCode, string message)
        : base(message) => ErrorCode = errorCode;

    /// <summary>Gets the VK error code, or 0 when VK sent none.</summary>
    public int ErrorCode { get; }
}
