using System;

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

                _isInit = true;
            }
        }
    }
}
