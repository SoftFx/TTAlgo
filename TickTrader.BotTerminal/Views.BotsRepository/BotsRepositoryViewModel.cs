using Caliburn.Micro;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Package;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotsRepositoryViewModel : Screen, IWindowModel
    {
        private readonly GitHubClient _client = new(new ProductHeaderValue("AndrewKhloptsau"));
        private readonly string _cacheFolder = Path.Combine(Environment.CurrentDirectory, "ReleaseCache");

        private readonly IAlgoAgent _agent;

        public string Name { get; }

        public ObservableCollection<ReleaseViewModel> Releases { get; } = new();

        public ObservableCollection<AssetViewModel> CurrentBots { get; } = new();


        public BotsRepositoryViewModel(IAlgoAgent agent)
        {
            DisplayName = "Bots repository";
            _agent = agent;

            Name = _agent.Name;

            _ = RefreshCollection();
        }


        public async Task RefreshCollection()
        {
            var release = await _client.Repository.Release.GetLatest("AndrewKhloptsau", "AlgoBots");

            Directory.CreateDirectory(_cacheFolder);

            var releaseFolder = Path.Combine(_cacheFolder, release.Name);

            if (!Directory.Exists(releaseFolder))
            {
                Directory.CreateDirectory(releaseFolder);

                foreach (var asset in release.Assets)
                {
                    var assetPath = Path.Combine(releaseFolder, asset.Name);

                    var response = await _client.Connection.Get<byte[]>(new Uri(asset.BrowserDownloadUrl), new Dictionary<string, string>(), "application/octet-stream");

                    using var fileStream = new FileStream(assetPath, System.IO.FileMode.Create, FileAccess.Write);

                    await fileStream.WriteAsync(response.Body.AsMemory(0, response.Body.Length));
                }
            }

            var releaseVm = new ReleaseViewModel
            {
                Name = release.Name,
            };

            foreach (var packagePath in Directory.GetFiles(releaseFolder))
            {
                var info = PackageLoadContext.ReflectionOnlyLoad(string.Empty, packagePath);

                foreach (var plugin in info.Plugins)
                {
                    var asset = new AssetViewModel
                    {
                        Name = plugin.Descriptor_.DisplayName,
                        Version = plugin.Descriptor_.Version,
                    };

                    releaseVm.Plugins.Add(asset);
                }
            }

            Releases.Add(releaseVm);

            LoadCurrentBots();
        }

        private void LoadCurrentBots()
        {
            foreach (var plugin in _agent.Plugins.Snapshot.Values)
            {
                if (plugin.Descriptor_.IsIndicator)
                    continue;

                var better = Releases[0].Plugins.FirstOrDefault(p => p.Name == plugin.Descriptor_.DisplayName && string.Compare(p.Version, plugin.Descriptor_.Version) == 1);

                var asset = new AssetViewModel
                {
                    Name = plugin.Descriptor_.DisplayName,
                    Version = plugin.Descriptor_.Version,
                };

                if (better is not null)
                    asset.SetBetterVersion(better.Version);

                CurrentBots.Add(asset);
            }
        }
    }
}