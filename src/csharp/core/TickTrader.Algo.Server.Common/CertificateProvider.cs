using System.IO;
using System.Reflection;

namespace TickTrader.Algo.Server.Common
{
    public static class CertificateProvider
    {
        public static string RootCertificate { get; private set; }

        public static string ServerCertificate { get; private set; }

        public static string ServerKey { get; private set; }


        public static void InitServer(string serverCertificate, string serverPrivateKey)
        {
            ServerCertificate = serverCertificate;
            ServerKey = serverPrivateKey;
        }

        public static void InitClient(Assembly assembly, string pathToCertificate)
        {
            using (var s = assembly.GetManifestResourceStream(pathToCertificate))
                using (var r = new StreamReader(s))
                {
                    RootCertificate = r.ReadToEnd();
                }
        }
    }
}
