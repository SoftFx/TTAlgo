using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using System.IO;
using TickTrader.DedicatedServer.DS.Models;
using System.Net;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TradeBotsController : Controller
    {
        private readonly ILogger<TradeBotsController> _logger;
        private readonly IDedicatedServer _dedicatedServer;

        public TradeBotsController(IDedicatedServer ddServer, ILogger<TradeBotsController> logger)
        {
            _dedicatedServer = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public TradeBotDto[] Get()
        {
            var bots = _dedicatedServer.TradeBots.ToArray();
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
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpGet("{botName}/[action]")]
        public string BotId(string botName)
        {
            return _dedicatedServer.AutogenerateBotId(WebUtility.UrlDecode(botName));
        }

        [HttpPost]
        public IActionResult Post([FromBody]PluginSetupDto setup)
        {
            try
            {
                var config = new TradeBotModelConfig
                {
                    InstanceId = setup.InstanceId,
                    Account = new AccountKey(setup.Account.Login, setup.Account.Server),
                    Plugin = new PluginKey(setup.PackageName, setup.PluginId),
                    PluginConfig = setup.Parse(),
                    Isolated = setup.Isolated
                };

                var tradeBot = _dedicatedServer.AddBot(config);

                return Ok(tradeBot.ToDto());
            }
            catch (DSException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]PluginSetupDto setup)
        {
            try
            {
                var tradeBot = GetBotOrThrow(WebUtility.UrlDecode(id));

                var pluginCfg = setup.Parse();
                setup.EnsureFiles(ServerModel.GetWorkingFolderFor(tradeBot.Id));

                tradeBot.Configurate(pluginCfg, setup.Isolated);

                return Ok();
            }
            catch (DSException ex)
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
                _dedicatedServer.RemoveBot(WebUtility.UrlDecode(id), clean_log, clean_algodata);

                return Ok();
            }
            catch (DSException ex)
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
            catch (DSException ex)
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
            catch (DSException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        private ITradeBot GetBotOrThrow(string id)
        {
            var tradeBot = _dedicatedServer.TradeBots.FirstOrDefault(tb => tb.Id == id);
            if (tradeBot == null)
                throw new BotNotFoundException($"Bot {id} not found");
            else
                return tradeBot;
        }
    }
}
