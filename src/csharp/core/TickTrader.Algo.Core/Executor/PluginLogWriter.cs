using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface IPluginLogWriter
    {
        void Start(string logDir);

        void Stop();

        void OnLogRecord(PluginLogRecord record);

        void OnStatusUpdate(PluginStatusUpdate update);

        void OnBotStarted();

        void OnBotStopped();
    }


    public static class PluginLogWriter
    {
        private static Func<IPluginLogWriter> _factory;


        public static TimeSpan StatusWriteTimeout { get; } = TimeSpan.FromMinutes(1);


        public static void Init(Func<IPluginLogWriter> factory) => _factory = factory;

        public static IPluginLogWriter Create() => _factory?.Invoke() ?? new NullPluginLogWriter();
    }


    public sealed class NullPluginLogWriter : IPluginLogWriter
    {
        public void Start(string logDir) { }

        public void Stop() { }

        public void OnLogRecord(PluginLogRecord record) { }

        public void OnStatusUpdate(PluginStatusUpdate update) { }

        public void OnBotStarted() { }

        public void OnBotStopped() { }
    }
}
