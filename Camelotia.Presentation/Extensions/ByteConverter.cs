﻿using System;
using System.Globalization;

namespace Camelotia.Presentation.Extensions
{
    public static class ByteConverter
    {
        public static string ByteSizeToString(this long byteCount, int decimalPrecision = 1)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1000)));
            var num = Math.Round(bytes / Math.Pow(1000, place), decimalPrecision);
            var digit = Math.Sign(byteCount) * num;
            return digit.ToString(CultureInfo.InvariantCulture) + suf[place];
        }
    }
}
