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

        [HttpGet("{id}/[Action]")]
        public IActionResult Log(string id)
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

        [HttpGet("{id}/[Action]")]
        public IActionResult Status(string id)
        {
            try
            {
                var tradeBot = GetBotOrThrow(id);

                return Ok(new BotStatusDto {
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
                    new PluginKey(setup.PackageName, setup.PluginId),
                    CreateConfig(setup));

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
                tradeBot.Configurate(CreateConfig(setup));

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

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                _dedicatedServer.RemoveBot(id);

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

        private PluginConfig CreateConfig(PluginSetupDto setup)
        {
            var barConfig = new BarBasedConfig()
            {
                MainSymbol = setup.Symbol,
                PriceType = BarPriceType.Ask
            };
            foreach (var param in setup.Parameters)
            {
                switch (param.DataType)
                {
                    case "Int":
                        barConfig.Properties.Add(new IntParameter() { Id = param.Id, Value = (int)(long)param.Value });
                        break;
                    case "Double":
                        switch (param.Value)
                        {
                            case Int64 l:
                                barConfig.Properties.Add(new DoubleParameter() { Id = param.Id, Value = (long)param.Value });
                                break;
                            case Double d:
                                barConfig.Properties.Add(new DoubleParameter() { Id = param.Id, Value = (double)param.Value });
                                break;
                            default: throw new InvalidCastException($"Can't cast {param.Value} to Double");
                        }
                        break;
                    case "String":
                        barConfig.Properties.Add(new StringParameter() { Id = param.Id, Value = (string)param.Value });
                        break;
                    case "Enum":
                        barConfig.Properties.Add(new EnumParameter() { Id = param.Id, Value = (string)param.Value });
                        break;
                }
            }

            return barConfig;
        }
    }
}
