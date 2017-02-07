using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.VS.Package
{
    internal static class EnvService
    {
        public const string ApiAssemblyName = "TickTrader.Algo.Api";
        public const string ApiAssemblyFileName = "TickTrader.Algo.Api.dll";

        public static string AlgoCommonFolder
        {
            get
            {
                string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(docFolder, "BotTrader");
            }
        }

        public static string PackageFolder { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }

        public static string AlgoCommonRepositoryFolder { get { return Path.Combine(AlgoCommonFolder, "AlgoRepository"); } }
        public static string AlgoCommonApiFolder { get { return Path.Combine(AlgoCommonFolder, "AlgoApi"); } }
    }
}
