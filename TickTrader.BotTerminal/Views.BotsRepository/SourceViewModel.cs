using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TickTrader.Algo.Package;
using TickTrader.Algo.Tools.MetadataBuilder;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class SourceViewModel
    {
        private readonly GitHubClient _client = new(new ProductHeaderValue("AndrewKhloptsau"));
        private readonly string _cacheFolder = Path.Combine(Environment.CurrentDirectory, "ReleaseCache");


        public string Name { get; init; }

        public string Link { get; init; }

        public ObservableCollection<BotInfoViewModel> BotsInfo { get; } = new();


        public void OpenSourceInBrowser()
        {
            try
            {
                var sInfo = new ProcessStartInfo(Link)
                {
                    UseShellExecute = true,
                };

                Process.Start(sInfo);
            }
            catch (Exception ex)
            {
                MessageBoxManager.OkError(ex.Message);
            }
        }

        internal async Task RefreshBotsInfo()
        {
            if (string.IsNullOrEmpty(Link))
                return;

            var release = await _client.Repository.Release.GetLatest("AndrewKhloptsau", "AlgoBots");

            Directory.CreateDirectory(_cacheFolder);

            var reposMetainfoAsset = release.Assets.FirstOrDefault(a => a.Name == "RepositoryInfo.json");


            if (reposMetainfoAsset != null)
            {
                var response = await _client.Connection.Get<byte[]>(new Uri(reposMetainfoAsset.BrowserDownloadUrl), new Dictionary<string, string>(), "application/octet-stream");

                using var stream = new MemoryStream(response.Body);

                var metainfo = await JsonSerializer.DeserializeAsync<MetadataInfo>(stream);

                foreach (var plugin in metainfo.Plugins)
                {
                    var asset = new BotInfoViewModel(plugin);

                    BotsInfo.Add(asset.ApplyPackage(metainfo));
                }
            }

            //var releaseFolder = Path.Combine(_cacheFolder, release.Name);

            //if (!Directory.Exists(releaseFolder))
            //{
            //    Directory.CreateDirectory(releaseFolder);

            //    foreach (var asset in release.Assets)
            //    {
            //        var assetPath = Path.Combine(releaseFolder, asset.Name);

            //        var response = await _client.Connection.Get<byte[]>(new Uri(asset.BrowserDownloadUrl), new Dictionary<string, string>(), "application/octet-stream");

            //        using var fileStream = new FileStream(assetPath, System.IO.FileMode.Create, FileAccess.Write);

            //        await fileStream.WriteAsync(response.Body.AsMemory(0, response.Body.Length));
            //    }
            //}

            //foreach (var packagePath in Directory.GetFiles(releaseFolder))
            //{
            //    var info = PackageLoadContext.ReflectionOnlyLoad(string.Empty, packagePath);

            //    foreach (var plugin in info.Plugins)
            //    {
            //        var asset = new BotInfoViewModel(plugin.Descriptor_.DisplayName);
            //        asset.ApplyPackage(plugin);
            //        BotsInfo.Add(asset);
            //    }
            //}
        }
    }
}