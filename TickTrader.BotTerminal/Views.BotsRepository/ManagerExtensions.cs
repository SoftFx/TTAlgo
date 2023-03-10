using Google.Protobuf.WellKnownTypes;
using System;
using TickTrader.Algo.Tools.MetadataBuilder;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal static class ManagerExtensions
    {
        internal static string ToKB(this long size) => $"{size / 1024L} KB";

        internal static string ToDefaultTime(this DateTime date) => date.ToString(MetadataInfo.DateTimeFormat);

        internal static string ToDefaultTime(this Timestamp date) => date.ToDateTime().ToDefaultTime();
    }
}