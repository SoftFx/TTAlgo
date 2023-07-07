﻿using System;
using System.IO;
using System.Reflection;

namespace TickTrader.Algo.Core.Lib
{
    public static class AssemblyExtensions
    {
        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            return GetLinkerTime(assembly.Location, target);
        }

        public static DateTime GetLinkerTime(string filePath, TimeZoneInfo target = null)
        {
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Read(buffer, 0, 2048);
            }

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            return target == null ? linkTimeUtc : TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, target);
        }
    }
}
