using System.IO;
using System.Reflection;

namespace TickTrader.Algo.ServerControl
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

        public static void InitClient()
        {
            var a = Assembly.GetExecutingAssembly();

            using (var s = a.GetManifestResourceStream("TickTrader.Algo.ServerControl.certs.algo-ca.crt"))
            using (var r = new StreamReader(s))
            {
                RootCertificate = r.ReadToEnd();
            }
        }
    }
}
