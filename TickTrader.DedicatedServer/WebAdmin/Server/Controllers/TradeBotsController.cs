using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Api;
using System.IO;
using TickTrader.DedicatedServer.DS.Models;
using Newtonsoft.Json.Linq;
using TickTrader.DedicatedServer.Extensions;

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
                var tradeBot = GetBotOrThrow(id);

                return Ok(tradeBot.ToDto());
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);
                tradeBot.Log.Clean();

                return Ok();
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);

                return Ok(tradeBot.Log.ToDto());
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);

                var readOnlyFile = tradeBot.Log.GetFile(file);

                return File(readOnlyFile.OpenRead(), MimeMipping.GetContentType(file), file);
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);
                tradeBot.Log.DeleteFile(file);

                return Ok();
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);

                var botWorkDir = Path.Combine(ServerModel.Environment.AlgoWorkingFolder, tradeBot.Id);
                var dirInfo = new DirectoryInfo(botWorkDir);

                var files = new FileDto[0];

                if (dirInfo.Exists)
                    files = dirInfo.GetFiles().Select(f => new FileDto { Name = f.Name, Size = f.Length }).ToArray();

                return Ok(files);
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);

                var filePath = Path.Combine(Path.Combine(ServerModel.Environment.AlgoWorkingFolder, tradeBot.Id), file);
                var readOnlyFile = new ReadOnlyFileModel(filePath);

                return File(readOnlyFile.OpenRead(), MimeMipping.GetContentType(file), file);
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);

                return Ok(new BotStatusDto
                {
                    Status = tradeBot.Log.Status,
                    BotId = tradeBot.Id
                });
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
            return _dedicatedServer.AutogenerateBotId(botName);
        }

        [HttpPost]
        public IActionResult Post([FromBody]PluginSetupDto setup)
        {
            try
            {
                var tradeBot = _dedicatedServer.AddBot(setup.InstanceId,
                    new AccountKey(setup.Account.Login, setup.Account.Server),
                    new PluginKey(setup.PackageName, setup.PluginId), setup.Parse(new WorkingDirectoryNamingStrategy(setup.InstanceId)));

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
                var tradeBot = GetBotOrThrow(id);
                tradeBot.Configurate(setup.Parse(new WorkingDirectoryNamingStrategy(id)));

                return Ok();
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
            }
            catch (DSException ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.ToBadResult());
            }
        }

        [HttpDelete]
        public IActionResult Delete(string id, bool clean_log = false, bool clean_algodata = false)
        {
            try
            {
                _dedicatedServer.RemoveBot(id, clean_log, clean_algodata);

                return Ok();
            }
            catch (InvalidStateException isex)
            {
                return BadRequest(isex.ToBadResult());
            }
        }

        [HttpPatch("{id}/[action]")]
        public IActionResult Start(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(id);
                tradeBot.Start();

                return Ok();
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
                var tradeBot = GetBotOrThrow(id);
                tradeBot.StopAsync();

                return Ok();
            }
            catch (BotNotFoundException nfex)
            {
                _logger.LogError(nfex.Message);
                return NotFound(nfex.ToBadResult());
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
