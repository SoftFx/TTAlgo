using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal.Model.AutoUpdate
{
    internal class GithubAppUpdateProvider : IAppUpdateProvider
    {
        private const int DefaultPageSize = 5;
        private const string DefaultClientName = "AlgoTerminal";

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<GithubAppUpdateProvider>();
        private static readonly TimeSpan ReleaseCacheResetTimeout = TimeSpan.FromDays(1);

        private readonly string _srcId, _url, _repoOwner, _repoName;
        private readonly GitHubClient _client = new(new ProductHeaderValue($"{DefaultClientName}"));
        private readonly Dictionary<string, ReleaseInfo> _releaseCache = new();

        private DateTime _releaseCacheLastReset;


        public GithubAppUpdateProvider(UpdateDownloadSource updateSrc)
        {
            _srcId = updateSrc.Name;
            _url = updateSrc.Uri;

            var uri = new Uri(updateSrc.Uri);
            _repoOwner = uri.Segments[1].Trim('/');
            _repoName = uri.Segments[2].Trim('/');
        }


        public Task<List<AppUpdateEntry>> GetUpdates() => Task.Run(() => GetUpdatesInternal());

        public async Task Download(string versionId, UpdateAssetTypes assetType, string dstPath)
        {
            if (!_releaseCache.TryGetValue(versionId, out var releaseInfo))
                throw new ArgumentException("Invalid version id");

            if (!releaseInfo.AssetsByType.TryGetValue(assetType, out var assetId))
                throw new ArgumentException("Invalid asset type");

            var asset = await _client.Repository.Release.GetAsset(_repoOwner, _repoName, assetId);
            if (asset.Size > 1 << 30) // 1GiB
                throw new Exception("Update size too large");
            var response = await LoadFileBytes(asset);
            await File.WriteAllBytesAsync(dstPath, response.Body);
        }


        private async Task<List<AppUpdateEntry>> GetUpdatesInternal()
        {
            var res = new List<AppUpdateEntry>();
            try
            {
                await LoadReleases();

                foreach (var release in _releaseCache.Values)
                {
                    res.Add(release.UpdateData);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get updates from github repo '{_url}'");
            }
            return res;
        }

        private async Task LoadReleases()
        {
            if (DateTime.UtcNow > _releaseCacheLastReset + ReleaseCacheResetTimeout)
            {
                _releaseCache.Clear();
                _releaseCacheLastReset = DateTime.UtcNow;
            }

            var page = 1;
            while (true)
            {
                var releases = await _client.Repository.Release.GetAll(_repoOwner, _repoName, new ApiOptions { StartPage = page, PageSize = DefaultPageSize });
                foreach (var release in releases)
                {
                    if (_releaseCache.ContainsKey(release.Id.ToString()))
                        return; // load only new releases. Rely on endpoint sort order

                    var releaseInfo = await TryReadReleaseInfo(release);
                    if (releaseInfo != null)
                        _releaseCache.Add(release.Id.ToString(), releaseInfo);
                }
                page++;
                if (releases.Count != DefaultPageSize)
                    return;
            }
        }

        private async Task<ReleaseInfo> TryReadReleaseInfo(Release release)
        {
            try
            {
                UpdateInfo updateInfo = null;
                var updateInfoAsset = release.Assets.FirstOrDefault(a => a.Name == UpdateHelper.InfoFileName);
                if (updateInfoAsset != null)
                {
                    var updateInfoBytes = await LoadFileBytes(updateInfoAsset);
                    using var memStream = new MemoryStream(updateInfoBytes.Body);
                    updateInfo = JsonSerializer.Deserialize<UpdateInfo>(memStream);
                }

                if (updateInfo != null)
                {
                    var assetsByType = new Dictionary<UpdateAssetTypes, int>();
                    foreach (var asset in release.Assets)
                    {
                        if (asset.Name.StartsWith("Algo") && asset.Name.EndsWith(".Setup.exe"))
                            assetsByType.Add(UpdateAssetTypes.Setup, asset.Id);
                        else if (asset.Name.StartsWith("AlgoTerminal") && asset.Name.EndsWith(".Update.zip"))
                            assetsByType.Add(UpdateAssetTypes.TerminalUpdate, asset.Id);
                        else if (asset.Name.StartsWith("AlgoServer") && asset.Name.EndsWith(".Update.zip"))
                            assetsByType.Add(UpdateAssetTypes.ServerUpdate, asset.Id);
                    }

                    return new ReleaseInfo
                    {
                        RepoData = release,
                        AssetsByType = assetsByType,
                        UpdateData = new AppUpdateEntry
                        {
                            SrcId = _srcId,
                            VersionId = release.Id.ToString(),
                            Info = updateInfo,
                            AvailableAssets = assetsByType.Keys.ToList(),
                        },
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to read release '{release.Url}'");
            }
            return null;
        }

        private Task<IApiResponse<byte[]>> LoadFileBytes(ReleaseAsset asset, CancellationToken? token = null)
        {
            return _client.Connection.Get<byte[]>(new Uri(asset.BrowserDownloadUrl), ImmutableDictionary<string, string>.Empty, "application/octet-stream", token ?? CancellationToken.None);
        }


        private class ReleaseInfo
        {
            public Release RepoData { get; set; }

            public AppUpdateEntry UpdateData { get; set; }

            public Dictionary<UpdateAssetTypes, int> AssetsByType { get; set; }
        }
    }
}
