// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Camelotia.Services.Apis;

/// <summary>Creates the random values an OAuth sign-in with PKCE (RFC 7636) needs.</summary>
internal static class Pkce
{
    /// <summary>The number of random bytes behind each string; 32 bytes give the 43 characters RFC 7636 asks for at least.</summary>
    private const int RandomByteCount = 32;

    /// <summary>Creates a random URL-safe string, for a code verifier or a state value. It is 43 characters long.</summary>
    /// <returns>The random string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string CreateRandomString() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(RandomByteCount));

    /// <summary>Derives the S256 code challenge from a code verifier.</summary>
    /// <param name="verifier">The code verifier.</param>
    /// <returns>The base64url-encoded SHA-256 hash of the verifier.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string CreateChallenge(string verifier) => Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
}
