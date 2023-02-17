using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TickTrader.Algo.Tools.MetadataBuilder;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class SourceViewModel
    {
        private readonly List<BotInfoViewModel> _botsInfo = new();

        private readonly GitHubClient _client = new(new ProductHeaderValue("AndrewKhloptsau"));
        private readonly string _cacheFolder = Path.Combine(Environment.CurrentDirectory, "ReleaseCache");


        public string Name { get; init; }

        public string Link { get; init; }

        public ObservableCollection<BotInfoViewModel> BotsInfo { get; } = new();


        public void OpenSourceInBrowser() => OpenLinkInBrowser(Link);

        private Task DownloadPackage(BotInfoViewModel model)
        {
            return Task.Delay(2000);
        }

        internal async Task RefreshBotsInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(Link))
                    return;

                _botsInfo.Clear();
                BotsInfo.Clear();

                var release = await _client.Repository.Release.GetLatest("AndrewKhloptsau", "AlgoBots");

                Directory.CreateDirectory(_cacheFolder);

                var reposMetainfoAsset = release.Assets.FirstOrDefault(a => a.Name == "RepositoryInfo.json");


                if (reposMetainfoAsset != null)
                {
                    var response = await _client.Connection.Get<byte[]>(new Uri(reposMetainfoAsset.BrowserDownloadUrl), new Dictionary<string, string>(), "application/octet-stream");

                    using var stream = new MemoryStream(response.Body);

                    var reposInfo = await JsonSerializer.DeserializeAsync<MetadataInfo[]>(stream);

                    foreach (var botInfo in reposInfo)
                    {
                        foreach (var plugin in botInfo.Plugins)
                        {
                            var asset = new BotInfoViewModel(plugin, DownloadPackage);

                            asset = asset.ApplyPackage(botInfo);

                            _botsInfo.Add(asset);
                            BotsInfo.Add(asset);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        internal bool UpdateVisibleBotsCollection(string filter)
        {
            BotsInfo.Clear();

            foreach (var botInfo in _botsInfo)
                if (botInfo.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    BotsInfo.Add(botInfo);

            return BotsInfo.Count > 0 || string.IsNullOrEmpty(filter);
        }


        internal static void OpenLinkInBrowser(string link)
        {
            try
            {
                if (link == null)
                    return;

                var sInfo = new ProcessStartInfo(link)
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
    }
}