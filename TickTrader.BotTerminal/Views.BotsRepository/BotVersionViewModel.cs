using System;
using System.Diagnostics;
using System.IO;
using TickTrader.Algo.Domain;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    public sealed class BotVersionViewModel
    {
        public string Version { get; }

        public string ApiVersion { get; }

        public string PackageId { get; }


        public string Size { get; }

        public string PackagePath { get; }

        public DateTime? LastModify { get; }

        public DateTime? CreatedDate { get; }

        internal PluginDescriptor Descriptior { get; }


        public BotVersionViewModel(string packageId, PluginDescriptor descriptor, PackageIdentity identity)
        {
            PackageId = packageId;
            Descriptior = descriptor;

            Version = descriptor.Version;
            ApiVersion = descriptor.ApiVersionStr;

            Size = $"{identity?.Size / 1024} KB";
            PackagePath = identity?.FilePath;
            LastModify = identity?.LastModifiedUtc.ToDateTime();
            CreatedDate = identity?.CreatedUtc.ToDateTime();
        }


        public void OpenPackageInExplorer()
        {
            var path = Path.GetDirectoryName(PackagePath);

            try
            {
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                MessageBoxManager.OkError(ex.Message);
            }
        }

        public void RemovePackageWithVersion()
        {
            if (MessageBoxManager.YesNoBoxQuestion("Are you sure you want to remove the selected package?"))
            {
                try
                {
                    File.Delete(PackagePath);
                }
                catch (Exception ex)
                {
                    MessageBoxManager.OkError(ex.Message);
                }
            }
        }
    }
}
