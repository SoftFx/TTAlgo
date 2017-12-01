using System;
using Sfx = SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    internal static class ToAlgo
    {
        internal static BotState Convert(Sfx.BotState state)
        {
            switch (state)
            {
                case Sfx.BotState.Offline:
                    return BotState.Offline;
                case Sfx.BotState.Starting:
                    return BotState.Starting;
                case Sfx.BotState.Faulted:
                    return BotState.Faulted;
                case Sfx.BotState.Online:
                    return BotState.Online;
                case Sfx.BotState.Stopping:
                    return BotState.Stopping;
                case Sfx.BotState.Broken:
                    return BotState.Broken;
                case Sfx.BotState.Reconnecting:
                    return BotState.Reconnecting;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
