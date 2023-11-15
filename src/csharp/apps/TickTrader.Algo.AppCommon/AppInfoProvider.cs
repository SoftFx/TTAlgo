using System;
using System.Text;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.AppCommon
{
    public static class AppInfoProvider
    {
        public const string DataFileName = "appinfo.json";

        private static readonly object _staticLock = new();

        private static bool _isInit;
        private static AppInfoResolved _data;


        public static AppInfoResolved Data
        {
            get
            {
                if (!_isInit)
                    Init();

                return _data;
            }
        }

        public static string DataPath => Data.DataPath;

        public static bool HasError => Data.HasError;

        public static Exception Error => Data.Error;


        public static void Init(ResolveAppInfoRequest request = null)
        {
            lock (_staticLock)
            {
                if (_isInit)
                    return;

                request ??= new ResolveAppInfoRequest();
                _data = AppInfoResolved.Create(request);
                PathHelper.EnsureDirectoryCreated(_data.DataPath);

                _isInit = true;
            }
        }

        public static string GetStatus()
        {
            var data = Data;
            var sb = new StringBuilder()
                .AppendLine("AppInfoProvider status:")
                .Append("DataPath=").AppendLine(data.DataPath)
                .Append("AppInfoFolder=").AppendLine(data.AppInfoFolder)
                .Append("IsInstallDir=").AppendLine(data.IsInstallDir.ToString())
                .Append("IsDevDir=").AppendLine(data.IsDevDir.ToString());
            if (data.Info == null)
            {
                sb.AppendLine("AppInfo=NotFound");
            }
            else
            {
                var info = data.Info;
                sb.Append("AppInfo.CfgVersion=").AppendLine(info.CfgVersion.ToString())
                    .Append("AppInfo.InstallId=").AppendLine(info.InstallId.ToString())
                    .Append("AppInfo.IsPortable=").AppendLine(info.IsPortable.ToString())
                    .Append("AppInfo.DataPath=").AppendLine(info.DataPath.ToString());
            }
            return sb.ToString();
        }
    }
}
