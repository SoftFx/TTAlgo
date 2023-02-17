using Octokit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Tools.MetadataBuilder;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class SourceViewModel
    {
        private const string DefaultOwnerName = "AndrewKhloptsau";
        private const string DefaultClientName = "AlgoTerminal";
        private const string DefaultRepositoryName = "AlgoBots";
        private const string DefaultMetainfoFileName = "RepositoryInfo.json";

        private readonly ConcurrentDictionary<string, Task<string>> _downloadAssetsTasks = new();
        private readonly Dictionary<string, string> _requestParams = new();
        private readonly List<BotInfoViewModel> _botsInfo = new();

        private readonly GitHubClient _client = new(new ProductHeaderValue(DefaultClientName));
        private readonly Task<string> _emptyTask = Task.FromResult<string>(default);

        private Release _lastRelease;


        public string Name { get; init; }

        public string Link { get; init; }

        public ObservableCollection<BotInfoViewModel> BotsInfo { get; } = new();


        public void OpenSourceInBrowser() => OpenLinkInBrowser(Link);


        internal async Task RefreshBotsInfo(CancellationToken token)
        {
            try
            {
                if (string.IsNullOrEmpty(Link))
                    return;

                _botsInfo.Clear();

                _lastRelease = await _client.Repository.Release.GetLatest(DefaultOwnerName, DefaultRepositoryName);

                var reposMetainfoAsset = _lastRelease.Assets.FirstOrDefault(a => a.Name == DefaultMetainfoFileName);

                if (reposMetainfoAsset != null)
                {
                    var response = await LoadFileBytes(reposMetainfoAsset, token);

                    using var stream = new MemoryStream(response.Body);

                    var reposInfo = await JsonSerializer.DeserializeAsync<MetadataInfo[]>(stream, cancellationToken: token);

                    foreach (var botInfo in reposInfo)
                    {
                        foreach (var plugin in botInfo.Plugins)
                        {
                            var asset = new BotInfoViewModel(plugin, DownloadPackage);

                            _botsInfo.Add(asset.ApplyPackage(botInfo));
                        }
                    }
                }

                UpdateVisibleBotsCollection(string.Empty);
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


        private Task<string> DownloadPackage(BotInfoViewModel model)
        {
            var packageName = model.PackageName;

            if (_downloadAssetsTasks.TryGetValue(packageName, out var task))
                return task;

            task = DownloadAsset(packageName);

            return _downloadAssetsTasks.TryAdd(packageName, task) ? task : _emptyTask;
        }

        private async Task<string> DownloadAsset(string package)
        {
            try
            {
                var asset = _lastRelease.Assets.FirstOrDefault(u => u.Name.StartsWith(package));

                if (asset != null)
                {
                    var response = await LoadFileBytes(asset);
                    var fileName = Path.Combine(EnvService.Instance.AlgoRepositoryFolder, asset.Name);

                    using var fileStream = new FileStream(fileName, System.IO.FileMode.Create, FileAccess.Write);

                    await fileStream.WriteAsync(response.Body.AsMemory(0, response.Body.Length));

                    return fileName;
                }

                return string.Empty;
            }
            finally
            {
                _downloadAssetsTasks.TryRemove(package, out _);
            }
        }

        private Task<IApiResponse<byte[]>> LoadFileBytes(ReleaseAsset asset, CancellationToken? token = null)
        {
            return _client.Connection.Get<byte[]>(new Uri(asset.BrowserDownloadUrl), _requestParams, "application/octet-stream", token ?? CancellationToken.None);
        }
    }
}