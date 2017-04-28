using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TickTrader.DedicatedServer.DS;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Extensions;
using TickTrader.DedicatedServer.WebAdmin.Server.Models;
using TickTrader.DedicatedServer.DS.Models;
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
            var tradeBot = (TradeBotModel)_dedicatedServer.TradeBots.FirstOrDefault(tb => tb.Id == id);

            if (tradeBot != null)
                return Ok(tradeBot.ToDto());
            else
                return NotFound();
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
                var bot = _dedicatedServer.TradeBots.FirstOrDefault(b => b.Id == id);
                if (bot != null)
                {
                    bot.Start();
                }
            }
            catch(DSException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpPatch("{id}/[action]")]
        public IActionResult Stop(string id)
        {
            try
            {
                var bot = _dedicatedServer.TradeBots.FirstOrDefault(b => b.Id == id);
                if (bot != null)
                {
                    bot.StopAsync();
                }
            }
            catch (DSException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        private PluginConfig CreateConfig(PluginSetupDto setup)
        {
            var barConfig = new BarBasedConfig();
            barConfig.MainSymbol = setup.Symbol;
            barConfig.PriceType = BarPriceType.Ask;
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
