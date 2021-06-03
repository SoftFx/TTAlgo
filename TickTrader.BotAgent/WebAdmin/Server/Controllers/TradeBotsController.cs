using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.BotAgent.BA;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.BA.Models;
using System.Net;
using TickTrader.Algo.Domain;
using System.Threading.Tasks;
using TickTrader.BotAgent.BA.Repository;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TradeBotsController : Controller
    {
        private readonly ILogger<TradeBotsController> _logger;
        private readonly IBotAgent _botAgent;

        public TradeBotsController(IBotAgent ddServer, ILogger<TradeBotsController> logger)
        {
            _botAgent = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<TradeBotDto[]> Get()
        {
            var bots = await _botAgent.GetBots();
            return bots.Select(b => b.ToDto()).ToArray();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var tradeBot = await _botAgent.GetBotInfo(WebUtility.UrlDecode(id));

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
                var log = await _botAgent.GetBotLog(botId);
                await log.Clear();

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
                var log = await _botAgent.GetBotLog(botId);

                return Ok(await log.ToDto());
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
                var log = await _botAgent.GetBotLog(botId);

                var decodedFile = WebUtility.UrlDecode(file);
                var readOnlyFile = await log.GetFile(decodedFile);

                return File(readOnlyFile.OpenRead(), MimeMipping.GetContentType(decodedFile), decodedFile);
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
                var log = await _botAgent.GetBotLog(botId);
                await log.DeleteFile(WebUtility.UrlDecode(file));

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
                var algoData = await _botAgent.GetAlgoData(botId);

                var files = (await algoData.GetFiles()).Select(f => f.ToDto()).ToArray();

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
                var algoData = await _botAgent.GetAlgoData(botId);

                var decodedFile = WebUtility.UrlDecode(file);
                var readOnlyFile = await algoData.GetFile(decodedFile);

                return File(readOnlyFile.OpenRead(), MimeMipping.GetContentType(decodedFile), decodedFile);
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
                var log = await _botAgent.GetBotLog(botId);

                return Ok(new BotStatusDto
                {
                    Status = await log.GetStatusAsync(),
                    BotId = botId
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
            return _botAgent.GenerateBotId(WebUtility.UrlDecode(botName));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]PluginSetupDto setup)
        {
            try
            {
                var pluginCfg = setup.Parse();
                var accountId = AccountId.Pack(setup.Account.Server, setup.Account.Login);

                pluginCfg.Key = new PluginKey(setup.PackageId, setup.PluginId);

                var tradeBot = await _botAgent.AddBot(new AddPluginRequest(accountId, pluginCfg));
                setup.EnsureFiles(ServerModel.GetWorkingFolderFor(tradeBot.InstanceId));

                return Ok(tradeBot.ToDto());
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]PluginSetupDto setup)
        {
            try
            {
                var botId = WebUtility.UrlDecode(id);

                var pluginCfg = setup.Parse();
                pluginCfg.InstanceId = botId;

                await _botAgent.ChangeBotConfig(new ChangePluginConfigRequest(botId, pluginCfg));
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
                await _botAgent.RemoveBot(new RemovePluginRequest(WebUtility.UrlDecode(id), clean_log, clean_algodata));

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
                await _botAgent.StartBot(new StartPluginRequest(WebUtility.UrlDecode(id)));

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
                _botAgent.StopBotAsync(new StopPluginRequest(WebUtility.UrlDecode(id)));

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
