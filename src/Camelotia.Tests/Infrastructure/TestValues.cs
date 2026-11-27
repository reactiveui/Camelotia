// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Security.Cryptography;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Shared values the tests use, so no test holds a credential as a literal.</summary>
public static class TestValues
{
    /// <summary>The length of each generated credential.</summary>
    private const int CredentialLength = 32;

    /// <summary>Gets an access token, generated per run.</summary>
    public static string Token { get; } = RandomNumberGenerator.GetHexString(CredentialLength, lowercase: true);

    /// <summary>Gets a second access token, generated per run.</summary>
    public static string OtherToken { get; } = RandomNumberGenerator.GetHexString(CredentialLength, lowercase: true);

    /// <summary>Gets a password, generated per run.</summary>
    public static string Password { get; } = RandomNumberGenerator.GetHexString(CredentialLength, lowercase: true);

    /// <summary>Gets the bytes the content tests send and expect back.</summary>
    public static byte[] Content { get; } = "abc"u8.ToArray();

    /// <summary>Gets a fixed point in time for created and modified dates.</summary>
    public static DateTimeOffset Moment { get; } = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
