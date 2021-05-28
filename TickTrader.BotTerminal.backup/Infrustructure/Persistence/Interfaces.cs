using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Infrustructure.Persistence
{
    public interface IPersistContext
    {
        IPersistNode GetNode(string nodeId, PersistModes mode);
    }

    public enum PersistModes
    {
        Static, // keep node settings when node is disposed
        Dynamic // delete node settings when node is disposed
    }

    public interface IPersistNode : IPersistContext, IDisposable
    {
        string Id { get; }
        PersistModes Mode { get; }
        void Init();
        void Init(Action<SettingsReader> loadAction, Action<SettingsWriter> saveAction);
    }

    public interface SettingsReader
    {
        string Get(string settingId, string defValue);
        int Get(string settingId, int defValue);
        double Get(string settingId, double defValue);
        bool Get(string settingId, bool defValue);
        T Get<T>(string settingId);
        bool TryGet<T>(string settingId, out T comlexVal);

        IEnumerable<string> NestedNodeIds { get; }
    }

    public interface SettingsWriter
    {
        void Set(string settingId, string val);
        void Set(string settingId, int val);
        void Set(string settingId, double val);
        void Set(string settingId, bool val);
        void Set(string settingId, object val);
    }
}
