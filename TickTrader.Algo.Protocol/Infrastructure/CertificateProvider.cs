using System.IO;
using System.Reflection;

namespace TickTrader.Algo.Protocol
{
    public static class CertificateProvider
    {
        public static string RootCertificate { get; private set; }

        public static string ServerCertificate { get; private set; }

        public static string ServerKey { get; private set; }

        public static string ClientCertificate { get; private set; }

        public static string ClientKey { get; private set; }


        public static void InitServer()
        {
            var a = Assembly.GetExecutingAssembly();

            using (var s = a.GetManifestResourceStream("TickTrader.Algo.Protocol.certs.bot-agent.crt"))
            using (var r = new StreamReader(s))
            {
                ServerCertificate = r.ReadToEnd();
            }

            using (var s = a.GetManifestResourceStream("TickTrader.Algo.Protocol.certs.bot-agent.key"))
            using (var r = new StreamReader(s))
            {
                ServerKey = r.ReadToEnd();
            }

            //using (var s = a.GetManifestResourceStream("TickTrader.Algo.Protocol.certs.algo-ca.crt"))
            //using (var r = new StreamReader(s))
            //{
            //    RootCertificate = r.ReadToEnd();
            //}
        }

        public static void InitClient()
        {
            var a = Assembly.GetExecutingAssembly();

            using (var s = a.GetManifestResourceStream("TickTrader.Algo.Protocol.certs.algo-ca.crt"))
            using (var r = new StreamReader(s))
            {
                RootCertificate = r.ReadToEnd();
            }

            //using (var s = a.GetManifestResourceStream("TickTrader.Algo.Protocol.certs.bot-terminal.crt"))
            //using (var r = new StreamReader(s))
            //{
            //    ClientCertificate = r.ReadToEnd();
            //}

            //using (var s = a.GetManifestResourceStream("TickTrader.Algo.Protocol.certs.bot-terminal.key"))
            //using (var r = new StreamReader(s))
            //{
            //    ClientKey = r.ReadToEnd();
            //}
        }
    }
}
