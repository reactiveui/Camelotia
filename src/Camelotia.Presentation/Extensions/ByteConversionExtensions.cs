// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Camelotia.Presentation.Extensions;

/// <summary>Formats byte counts for display.</summary>
public static class ByteConversionExtensions
{
    /// <summary>The step between units; Camelotia uses decimal units, so a kilobyte is 1000 bytes.</summary>
    private const int UnitStep = 1000;

    /// <summary>The number of decimal places when the caller does not choose one.</summary>
    private const int DefaultPrecision = 1;

    /// <summary>The unit suffixes, from bytes up.</summary>
    private static readonly string[] Suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    /// <summary>Extends a byte count.</summary>
    /// <param name="byteCount">The number of bytes.</param>
    extension(long byteCount)
    {
        /// <summary>Formats the byte count in the largest unit below it, with one decimal place.</summary>
        /// <returns>The formatted size, such as <c>1.5KB</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ByteSizeToString() => byteCount.ByteSizeToString(DefaultPrecision);

        /// <summary>Formats the byte count in the largest unit below it.</summary>
        /// <param name="decimalPrecision">The number of decimal places.</param>
        /// <returns>The formatted size, such as <c>1.5KB</c>.</returns>
        public string ByteSizeToString(int decimalPrecision)
        {
            if (byteCount == 0)
            {
                return $"0{Suffixes[0]}";
            }

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, UnitStep)));
            var number = Math.Round(bytes / Math.Pow(UnitStep, place), decimalPrecision);
            var digit = Math.Sign(byteCount) * number;
            return digit.ToString(CultureInfo.InvariantCulture) + Suffixes[place];
        }
    }
}
