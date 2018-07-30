TTAlgo ssl keys and certificates generation routine. Generated keys and certificates are saved separately.

To update:
1. Generate new keys and certificates calling ssl-gen.bat
2. Replace bot-terminal-sign.pfx in TickTrader.BotTerminal/
3. Replace ManifestCertificateThumbprint in TickTrader.BotTerminal.csproj
4. Replace bot-agent.pfx in TickTrader.BotAgent/
5. Replace all files in TickTrader.Algo.Protocol/certs/