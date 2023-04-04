using System;
using System.Collections;
using System.Collections.Generic;

namespace TickTrader.Algo.AppCommon.Update
{
    public class UpdaterParams
    {
        public const string EnvVarPrefix = "TTAlgoUpdater_";
        public const string AppTypeVarName = EnvVarPrefix + nameof(AppType);
        public const string InstallPathVarName = EnvVarPrefix + nameof(InstallPath);
        public const string UpdatePathVarName = EnvVarPrefix + nameof(UpdatePath);


        public UpdateAppTypes? AppType { get; set; }

        public string InstallPath { get; set; }

        public string UpdatePath { get; set; } // debug param


        public void SaveAsEnvVars(IDictionary<string, string> envVars)
        {
            envVars.Add(AppTypeVarName, AppType?.ToString() ?? "");
            envVars.Add(InstallPathVarName, InstallPath);
        }

        public static UpdaterParams ParseFromEnvVars(IDictionary envVars)
        {
            var res = new UpdaterParams();

            if (TryGetString(envVars, AppTypeVarName, out var appTypeStr) && Enum.TryParse<UpdateAppTypes>(appTypeStr, out var appType))
                res.AppType = appType;
            if (TryGetString(envVars, InstallPathVarName, out var installPathStr))
                res.InstallPath = installPathStr;
            if (TryGetString(envVars, UpdatePathVarName, out var updatePathStr))
                res.UpdatePath = updatePathStr;

            return res;
        }


        private static bool TryGetString(IDictionary map, string key, out string strValue)
        {
            strValue = map[key] as string;
            return strValue != null;
        }
    }
}
