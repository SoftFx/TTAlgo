using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TradeBotsController : Controller
    {
        private readonly ILogger<TradeBotsController> _logger;
        private readonly IAlgoServerLocal _algoServer;

        public TradeBotsController(IAlgoServerLocal algoServer, ILogger<TradeBotsController> logger)
        {
            _algoServer = algoServer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<TradeBotDto[]> Get()
        {
            var snapshot = await _algoServer.GetPlugins();
            return snapshot.Plugins.Select(b => b.ToDto()).ToArray();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var tradeBot = await _algoServer.GetPluginInfo(WebUtility.UrlDecode(id));

                return Ok(tradeBot.ToDto());
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        #region Logs
        [HttpDelete("{id}/Logs")]
        public async Task<IActionResult> DeleteLogs(string id)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);
                await _algoServer.ClearPluginFolder(new ClearPluginFolderRequest(botId, PluginFolderInfo.Types.PluginFolderId.BotLogs));

                return Ok();
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpGet("{id}/[Action]")]
        public async Task<IActionResult> Logs(string id)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);

                var logsRes = await _algoServer.GetPluginLogs(new PluginLogsRequest { PluginId = botId, MaxCount = 100 });
                var folderInfo = await _algoServer.GetPluginFolderInfo(new PluginFolderInfoRequest(botId, PluginFolderInfo.Types.PluginFolderId.BotLogs));

                if (string.IsNullOrEmpty(logsRes.PluginId))
                    return BadRequest(new BadRequestResultDto(0, $"Bot '{botId}' not found"));

                var res = new TradeBotLogDto
                {
                    Snapshot = logsRes.Logs.OrderByDescending(le => le.TimeUtc).Select(e => e.ToDto()).ToArray(),
                    Files = folderInfo.Files.Select(f => f.ToDto()).ToArray(),
                };

                return Ok(res);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpGet("{id}/[Action]/{file}")]
        public async Task<IActionResult> Logs(string id, string file)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);
                var decodedFile = WebUtility.UrlDecode(file);

                var filePath = await _algoServer.GetPluginFileReadPath(new DownloadPluginFileRequest(botId, PluginFolderInfo.Types.PluginFolderId.BotLogs, decodedFile));

                return File(FileHelper.OpenSharedRead(filePath), MimeMipping.GetContentType(decodedFile), decodedFile);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpDelete("{id}/Logs/{file}")]
        public async Task<IActionResult> DeleteLog(string id, string file)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);
                var decodedFile = WebUtility.UrlDecode(file);
                await _algoServer.DeletePluginFile(new DeletePluginFileRequest(botId, PluginFolderInfo.Types.PluginFolderId.BotLogs, decodedFile));

                return Ok();
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }
        #endregion

        #region AlgoData
        [HttpGet("{id}/[Action]")]
        public async Task<IActionResult> AlgoData(string id)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);

                var folderInfo = await _algoServer.GetPluginFolderInfo(new PluginFolderInfoRequest(botId, PluginFolderInfo.Types.PluginFolderId.AlgoData));

                var files = folderInfo.Files.Select(f => f.ToDto()).ToArray();

                return Ok(files);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpGet("{id}/[Action]/{file}")]
        public async Task<IActionResult> AlgoData(string id, string file)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);
                var decodedFile = WebUtility.UrlDecode(file);

                var filePath = await _algoServer.GetPluginFileReadPath(new DownloadPluginFileRequest(botId, PluginFolderInfo.Types.PluginFolderId.AlgoData, decodedFile));

                return File(FileHelper.OpenSharedRead(filePath), MimeMipping.GetContentType(decodedFile), decodedFile);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }
        #endregion

        [HttpGet("{id}/[Action]")]
        public async Task<IActionResult> Status(string id)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);
                var statusRes = await _algoServer.GetPluginStatus(new PluginStatusRequest { PluginId = botId });

                if (string.IsNullOrEmpty(statusRes.PluginId))
                    return BadRequest(new BadRequestResultDto(0, $"Bot '{botId}' not found"));

                return Ok(new BotStatusDto
                {
                    Status = statusRes.Status,
                    BotId = botId,
                });
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpGet("{botName}/[action]")]
        public Task<string> BotId(string botName)
        {
            return _algoServer.GeneratePluginId(WebUtility.UrlDecode(botName));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PluginSetupDto setup)
        {
            try
            {
                var pluginCfg = setup.Parse();
                var accountId = AccountId.Pack(setup.Account.Server, setup.Account.Login);

                pluginCfg.Key = new PluginKey(setup.PackageId, setup.PluginId);

                await _algoServer.AddPlugin(new AddPluginRequest(accountId, pluginCfg));
                var botId = setup.InstanceId;
                setup.EnsureFiles(ServerModel.GetWorkingFolderFor(botId));

                var tradeBot = await _algoServer.GetPluginInfo(botId);

                return Ok(tradeBot.ToDto());
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] PluginSetupDto setup)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);

                var pluginCfg = setup.Parse();

                pluginCfg.InstanceId = botId;
                pluginCfg.Key = new PluginKey(setup.PackageId, setup.PluginId);

                await _algoServer.UpdatePluginConfig(new ChangePluginConfigRequest(botId, pluginCfg));
                setup.EnsureFiles(ServerModel.GetWorkingFolderFor(botId));

                return Ok();
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] bool clean_log = true, [FromQuery] bool clean_algodata = true)
        {
            try
            {
                await _algoServer.RemovePlugin(new RemovePluginRequest(WebUtility.UrlDecode(id), clean_log, clean_algodata));

                return Ok();
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpPatch("{id}/[action]")]
        public async Task<IActionResult> Start(string id)
        {
            try
            {
                await _algoServer.StartPlugin(new StartPluginRequest(WebUtility.UrlDecode(id)));

                return Ok();
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpPatch("{id}/[action]")]
        public IActionResult Stop(string id)
        {
            try
            {
                _algoServer.StopPlugin(new StopPluginRequest(WebUtility.UrlDecode(id)));

                return Ok();
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }
    }
}
