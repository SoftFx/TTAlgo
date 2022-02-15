using System;
using System.Collections;
using System.Collections.Generic;

namespace TickTrader.Algo.Rpc
{
    public class RpcProxyParams
    {
        public const string EnvVarPrefix = "TTAlgoRpc_";
        public const string AddressEnvVarName = EnvVarPrefix + nameof(Address);
        public const string PortEnvVarName = EnvVarPrefix + nameof(Port);
        public const string ProxyIdEnvVarName = EnvVarPrefix + nameof(ProxyId);
        public const string ParentProcIdVarName = EnvVarPrefix + nameof(ParentProcId);


        public string Address { get; set; }

        public int Port { get; set; }

        public string ProxyId { get; set; }

        public int? ParentProcId { get; set; }


        public static RpcProxyParams ReadFromEnvVars(IDictionary environment)
        {
            var envVars = new Dictionary<string, string>();
            TryGetString(environment, AddressEnvVarName, envVars);
            TryGetString(environment, PortEnvVarName, envVars);
            TryGetString(environment, ProxyIdEnvVarName, envVars);
            TryGetString(environment, ParentProcIdVarName, envVars);
            return ParseFromEnvVars(envVars);
        }

        public static RpcProxyParams ParseFromEnvVars(IDictionary<string, string> environment)
        {
            var res = new RpcProxyParams();

            res.Address = GetEnvVar(environment, AddressEnvVarName);
            res.ProxyId = GetEnvVar(environment, ProxyIdEnvVarName);
            
            if (!int.TryParse(GetEnvVar(environment, PortEnvVarName), out var port))
                throw new ArgumentException($"Environment variable '{PortEnvVarName}' has invalid value");
            res.Port = port;

            if (environment.TryGetValue(ParentProcIdVarName, out var procIdValue))
            {
                if (!int.TryParse(procIdValue, out var procId))
                    throw new ArgumentException($"Environment variable '{ParentProcIdVarName}' has invalid value");
                res.ParentProcId = procId;
            }

            return res;
        }


        public void SaveAsEnvVars(IDictionary<string, string> environment)
        {
            environment.Add(AddressEnvVarName, Address);
            environment.Add(PortEnvVarName, Port.ToString());
            environment.Add(ProxyIdEnvVarName, ProxyId);
            if (ParentProcId != null)
                environment.Add(ParentProcIdVarName, ParentProcId.ToString());
        }


        private static string GetEnvVar(IDictionary<string, string> environment, string envVarName)
        {
            if (!environment.TryGetValue(envVarName, out var envVarValue))
                throw new ArgumentException($"Environment variable '{envVarName}' not found");

            return envVarValue;
        }

        private static void TryGetString(IDictionary map, string key, IDictionary<string, string> typedMap)
        {
            var strValue = map[key] as string;
            if (strValue != null)
            {
                typedMap.Add(key, strValue);
            }
        }
    }
}
