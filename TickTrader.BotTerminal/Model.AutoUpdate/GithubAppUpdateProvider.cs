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

        public async Task Download(string subLink, string dstPath)
        {
            var asset = await _client.Repository.Release.GetAsset(_repoOwner, _repoName, int.Parse(subLink));
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
                    var assets = release.RepoData.Assets;
                    foreach (var asset in assets)
                    {
                        UpdateAppTypes? appType = null;
                        if (asset.Name.StartsWith("AlgoTerminal") && asset.Name.EndsWith(".Update.zip"))
                            appType = UpdateAppTypes.Terminal;
                        if (asset.Name.StartsWith("AlgoServer") && asset.Name.EndsWith(".Update.zip"))
                            appType = UpdateAppTypes.Server;

                        if (appType.HasValue)
                        {
                            res.Add(new AppUpdateEntry
                            {
                                SrcId = _srcId,
                                SubLink = asset.Id.ToString(),
                                Info = release.UpdateData,
                                AppType = appType.Value,
                            });
                        }
                    }
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
                    if (_releaseCache.ContainsKey(release.Url))
                        return; // load only new releases. Rely on endpoint sort order

                    var updateInfo = await TryLoadUpdateInfo(release);
                    if (updateInfo != null)
                        _releaseCache.Add(release.Url, new ReleaseInfo { RepoData = release, UpdateData = updateInfo });
                }
                page++;
                if (releases.Count != DefaultPageSize)
                    return;
            }
        }

        private async Task<UpdateInfo> TryLoadUpdateInfo(Release release)
        {
            try
            {
                var updateInfoAsset = release.Assets.FirstOrDefault(a => a.Name == UpdateHelper.InfoFileName);
                if (updateInfoAsset != null)
                {
                    var updateInfoBytes = await LoadFileBytes(updateInfoAsset);
                    using var memStream = new MemoryStream(updateInfoBytes.Body);
                    return JsonSerializer.Deserialize<UpdateInfo>(memStream);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to load update info for release '{release.Url}'");
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

            public UpdateInfo UpdateData { get; set; }
        }
    }
}
