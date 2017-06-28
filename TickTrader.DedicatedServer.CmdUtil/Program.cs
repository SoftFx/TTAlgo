using System;
using System.IO;

namespace TickTrader.DedicatedServer.CmdUtil
{
    class Program
    {
        private const string AppName = "AppName=";
        private const string InstallDir = "InstallDir=";
        private const string AppDir = "AppDir=";
        private const string AppExe = "AppExe=";
        private const string InstallerName = "Installer=";
        private const string Brand = "Brand=";
        private const string License = "License=";
        private const string ServiceName = "Service=";
        private const string ServiceDisplayName = "ServiceDispName=";

        static void Main(string[] args)
        {
            Console.WriteLine("Working Directory: {0}", Directory.GetCurrentDirectory());

            var scriptBuilder = new InstallScriptBuilder();

            scriptBuilder.UseApplicationName(ReadAppName(args))
                .UseApplicationDir(ReadAppDir(args))
                .UseApplicationExe(ReadAppExe(args))
                .UseInstallDir(ReadInstallDir(args))
                .UseInstallerName(ReadInstallerName(args))
                .UseLicense(ReadLicense(args))
                .UseServiceName(ReadServiceName(args))
                .UseServiceDisplayName(ReadServiceDisplayName(args))
                .UseBrand(ReadBrand(args));
            try
            {
                var script = scriptBuilder.Build();

                var scriptFile = Path.Combine(Directory.GetCurrentDirectory(), $"{ReadAppName(args)}.nsi");
                File.WriteAllText(scriptFile, script);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string ReadServiceName(string[] args)
        {
            return args.Read(ServiceName);
        }

        private static string ReadServiceDisplayName(string[] args)
        {
            return args.Read(ServiceDisplayName);
        }

        private static string ReadAppExe(string[] args)
        {
            return args.Read(AppExe);
        }

        private static string ReadBrand(string[] args)
        {
            return args.Read(Brand);
        }

        private static string ReadLicense(string[] args)
        {
            return args.Read(License);
        }
        private static string ReadInstallerName(string[] args)
        {
            return args.Read(InstallerName);
        }
        private static string ReadAppDir(string[] args)
        {
            return args.Read(AppDir);
        }
        private static string ReadInstallDir(string[] args)
        {
            return args.Read(InstallDir);
        }
        private static string ReadAppName(string[] args)
        {
            return args.Read(AppName);
        }

        private static bool IsAbsolutePath(string path)
        {
            return Path.GetFullPath(path) == path;
        }
    }
}
