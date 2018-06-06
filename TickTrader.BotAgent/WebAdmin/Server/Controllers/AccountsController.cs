using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IBotAgent _botAgent;

        public AccountsController(IBotAgent ddServer, ILogger<PackagesController> logger)
        {
            _botAgent = ddServer;
            _logger = logger;
        }

        [HttpGet]
        public AccountDto[] Get()
        {
            return _botAgent.GetAccounts().Select(a => a.ToDto()).ToArray();
        }

        [HttpGet("{server}/{login}/[action]")]
        public IActionResult Info(string server, string login)
        {
            try
            {
                var connError = _botAgent.GetAccountMetadata(new AccountKey(WebUtility.UrlDecode(server), WebUtility.UrlDecode(login)), out AccountMetadataInfo info);

                if (connError.Code == ConnectionErrorCodes.None)
                {
                    return Ok(info.ToDto());
                }
                else
                {
                    var communicationExc = new CommunicationException($"Connection error: {connError.Code}", connError.Code);

                    _logger.LogError(communicationExc.Message);

                    return BadRequest(communicationExc.ToBadResult());
                }
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]AccountDto account)
        {
            try
            {
                _botAgent.AddAccount(new AccountKey(account.Server, account.Login), account.Password, account.UseNewProtocol);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(string login, string server)
        {
            try
            {
                _botAgent.RemoveAccount(new AccountKey(server ?? "", login ?? ""));
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpPatch("[action]")]
        public IActionResult UpdatePassword([FromBody] AccountDto account)
        {
            try
            {
                _botAgent.ChangeAccountPassword(new AccountKey(account.Server, account.Login), account.Password);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpPatch("[action]")]
        public IActionResult ChangeProtocol([FromBody] AccountDto account)
        {
            try
            {
                _botAgent.ChangeAccountProtocol(new AccountKey(account.Server, account.Login));
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult Test(string login, string server, string password, bool useNewProtocol)
        {
            try
            {
                var testResult = string.IsNullOrWhiteSpace(password) ?
                    _botAgent.TestAccount(new AccountKey(server, login)) :
                    _botAgent.TestCreds(new AccountKey(server, login), password, useNewProtocol);

                return Ok(testResult);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }
        }
    }
}
