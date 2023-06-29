using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon.Update;

namespace TickTrader.BotTerminal.Model.AutoUpdate
{
    internal class AppUpdateEntry
    {
        public string SrcId { get; set; }

        public string SubLink { get; set; }

        public UpdateInfo Info { get; set; }

        public UpdateAppTypes AppType { get; set; }
    }

    internal interface IAppUpdateProvider
    {
        Task<List<AppUpdateEntry>> GetUpdates();

        Task Download(string subLink, string dstPath);
    }


    internal static class AppUpdateProvider
    {
        public static IAppUpdateProvider Create(UpdateDownloadSource updateSrc)
        {
            var uri = updateSrc.Uri;

            //if (uri.StartsWith("https://github.com"))
            //    return new GithubAppUpdateProvider(uri);

            if (Directory.Exists(uri))
                return new DirectoryAppUpdateProvider(updateSrc);

            throw new NotSupportedException("Uri is not recognised as supported");
        }
    }
}
