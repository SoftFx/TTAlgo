using System;
using Sfx = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    internal static class ToSfx
    {
        internal static Sfx.BotState Convert(BotState state)
        {
            switch (state)
            {
                case BotState.Offline:
                    return Sfx.BotState.Offline;
                case BotState.Starting:
                    return Sfx.BotState.Starting;
                case BotState.Faulted:
                    return Sfx.BotState.Faulted;
                case BotState.Online:
                    return Sfx.BotState.Online;
                case BotState.Stopping:
                    return Sfx.BotState.Stopping;
                case BotState.Broken:
                    return Sfx.BotState.Broken;
                case BotState.Reconnecting:
                    return Sfx.BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }
    }
}