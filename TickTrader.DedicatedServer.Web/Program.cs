using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using TickTrader.DedicatedServer.WebAdmin;

namespace TickTrader.DedicatedServer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("https://localhost:2016/")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<WebAdminStartup>()
                .Build();

            host.Run();
        }
    }
}
