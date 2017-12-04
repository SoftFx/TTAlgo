﻿using System;
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

        internal static LoginRejectReason Convert(Sfx.LoginRejectReason reason)
        {
            switch (reason)
            {
                case Sfx.LoginRejectReason.InvalidCredentials:
                    return LoginRejectReason.InvalidCredentials;
                case Sfx.LoginRejectReason.InternalServerError:
                    return LoginRejectReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }

        internal static LogoutReason Convert(Sfx.LogoutReason reason)
        {
            switch (reason)
            {
                case Sfx.LogoutReason.ClientRequest:
                    return LogoutReason.ClientRequest;
                case Sfx.LogoutReason.ServerLogout:
                    return LogoutReason.ServerLogout;
                case Sfx.LogoutReason.InternalServerError:
                    return LogoutReason.InternalServerError;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
