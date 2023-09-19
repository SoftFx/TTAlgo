using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon.Update;

namespace TickTrader.Algo.AutoUpdate
{
    public enum UpdateAssetTypes
    {
        Setup = 0,
        TerminalUpdate = 1,
        ServerUpdate = 2,
    }

    public class AppUpdateEntry
    {
        public string SrcId { get; set; }

        public string VersionId { get; set; }

        public bool IsStable { get; set; }

        public UpdateInfo Info { get; set; }

        public List<UpdateAssetTypes> AvailableAssets { get; set; }
    }

    public interface IAppUpdateProvider
    {
        IEnumerable<AppUpdateEntry> Updates { get; }

        string LoadUpdatesError { get; }


        Task LoadUpdates();

        AppUpdateEntry GetUpdate(string versionId);

        Task Download(string versionId, UpdateAssetTypes assetType, string dstPath);
    }


    public static class AppUpdateProvider
    {
        public static IAppUpdateProvider Create(UpdateDownloadSource updateSrc)
        {
            var uri = updateSrc.Uri;

            if (uri.StartsWith("https://github.com"))
                return new GithubAppUpdateProvider(updateSrc);

            if (Directory.Exists(uri))
                return new DirectoryAppUpdateProvider(updateSrc);

            throw new NotSupportedException("Uri is not recognised as supported");
        }
    }
}
