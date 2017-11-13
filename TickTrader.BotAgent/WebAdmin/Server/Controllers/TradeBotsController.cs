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
using TickTrader.BotAgent.BA.Builders;
using TickTrader.Algo.Core;

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
        public TradeBotDto[] Get()
        {
            var bots = _botAgent.TradeBots.ToArray();
            return bots.Select(b => b.ToDto()).ToArray();
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                return Ok(tradeBot.ToDto());
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        #region Logs
        [HttpDelete("{id}/Logs")]
        public IActionResult DeleteLogs(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));
                tradeBot.Log.Clear();

                return Ok();
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpGet("{id}/[Action]")]
        public IActionResult Logs(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                return Ok(tradeBot.Log.ToDto());
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpGet("{id}/[Action]/{file}")]
        public IActionResult Logs(string id, string file)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                var decodedFile = WebUtility.UrlDecode(file);
                var readOnlyFile = tradeBot.Log.GetFile(decodedFile);

                return File(readOnlyFile.OpenRead(), MimeMipping.GetContentType(decodedFile), decodedFile);
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpDelete("{id}/Logs/{file}")]
        public IActionResult DeleteLog(string id, string file)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));
                tradeBot.Log.DeleteFile(WebUtility.UrlDecode(file));

                return Ok();
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }
        #endregion

        #region AlgoData
        [HttpGet("{id}/[Action]")]
        public IActionResult AlgoData(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                var files = tradeBot.AlgoData.Files.Select(f => f.ToDto()).ToArray();

                return Ok(files);
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpGet("{id}/[Action]/{file}")]
        public IActionResult AlgoData(string id, string file)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));
                var decodedFile = WebUtility.UrlDecode(file);
                var readOnlyFile = tradeBot.AlgoData.GetFile(decodedFile);

                return File(readOnlyFile.OpenRead(), MimeMipping.GetContentType(decodedFile), decodedFile);
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }
        #endregion

        [HttpGet("{id}/[Action]")]
        public IActionResult Status(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                return Ok(new BotStatusDto
                {
                    Status = tradeBot.Log.Status,
                    BotId = tradeBot.Id
                });
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpGet("{botName}/[action]")]
        public string BotId(string botName)
        {
            return _botAgent.AutogenerateBotId(WebUtility.UrlDecode(botName));
        }

        [HttpPost]
        public IActionResult Post([FromBody]PluginSetupDto setup)
        {
            try
            {
                var pluginCfg = setup.Parse();

                var config = new TradeBotModelConfig
                {
                    InstanceId = setup.InstanceId,
                    Account = new AccountKey(setup.Account.Login, setup.Account.Server),
                    Plugin = new PluginKey(setup.PackageName, setup.PluginId),
                    PluginConfig = pluginCfg,
                    Isolated = setup.Isolated,
                    Permissions = ConvertToPluginPermissions(setup.Permissions)
                };

                var tradeBot = _botAgent.AddBot(config);
                setup.EnsureFiles(ServerModel.GetWorkingFolderFor(tradeBot.Id));

                return Ok(tradeBot.ToDto());
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        private PluginPermissions ConvertToPluginPermissions(PermissionsDto permissions)
        {
            if (permissions != null)
                return new PluginPermissions
                {
                    TradeAllowed = permissions.TradeAllowed
                };

            return new DefaultPermissionsBuilder().Build();
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]PluginSetupDto setup)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                var pluginCfg = setup.Parse();
                var config = new TradeBotModelConfig
                {
                    PluginConfig = pluginCfg,
                    Isolated = setup.Isolated,
                    Permissions = ConvertToPluginPermissions(setup.Permissions)
                };

                tradeBot.Configurate(config);
                setup.EnsureFiles(ServerModel.GetWorkingFolderFor(tradeBot.Id));

                return Ok();
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id, [FromQuery] bool clean_log = true, [FromQuery] bool clean_algodata = true)
        {
            try
            {
                _botAgent.RemoveBot(WebUtility.UrlDecode(id), clean_log, clean_algodata);

                return Ok();
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpPatch("{id}/[action]")]
        public IActionResult Start(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));
                tradeBot.Start();

                return Ok();
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpPatch("{id}/[action]")]
        public IActionResult Stop(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));
                tradeBot.StopAsync();

                return Ok();
            }
            catch (BAException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        private ITradeBot GetBotOrThrow(string id)
        {
            var tradeBot = _botAgent.TradeBots.FirstOrDefault(tb => tb.Id == id);
            if (tradeBot == null)
                throw new BotNotFoundException($"Bot {id} not found");
            else
                return tradeBot;
        }
    }
}
