﻿using System.Security.Cryptography.X509Certificates;

namespace TickTrader.Algo.ServerControl
{
    public interface IServerSettings
    {
        string ServerName { get; }

        X509Certificate2 Certificate { get; }

        IProtocolSettings ProtocolSettings { get; }
    }
}
