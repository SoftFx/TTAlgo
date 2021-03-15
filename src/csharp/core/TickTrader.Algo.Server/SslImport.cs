using System.IO;
using System.Reflection;

namespace TickTrader.Algo.Server
{
    public class SslImport
    {
        public static string LoadServerCertificate()
        {
            var a = Assembly.GetExecutingAssembly();

            using (var s = a.GetManifestResourceStream("TickTrader.Algo.Server.certs.bot-agent.crt"))
            using (var r = new StreamReader(s))
            {
                return r.ReadToEnd();
            }
        }

        public static string LoadServerPrivateKey()
        {
            var a = Assembly.GetExecutingAssembly();

            using (var s = a.GetManifestResourceStream("TickTrader.Algo.Server.certs.bot-agent.crt"))
            using (var r = new StreamReader(s))
            {
                return r.ReadToEnd();
            }
        }
    }
}
