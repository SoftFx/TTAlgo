using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Runtime
{
    public interface IRuntimeOwner
    {
        string RuntimeExePath { get; }

        string WorkingDirectory { get; }

        bool EnableDevMode { get; }

        RpcProxyParams GetRpcParams();

        void OnRuntimeStopped(string runtimeId);

        void OnRuntimeInvalid(string pkgId, string runtimeId);
    }
}
