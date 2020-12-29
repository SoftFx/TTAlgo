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
using TickTrader.Algo.Domain;
using System.Threading.Tasks;

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
        public async Task<AccountDto[]> Get()
        {
            var accounts = await _botAgent.GetAccounts();
            return accounts.Select(a => a.ToDto()).ToArray();
        }

        [HttpGet("{server}/{login}/[action]")]
        public async Task<IActionResult> Info(string server, string login)
        {
            try
            {
                var res = await _botAgent.GetAccountMetadata(new AccountKey(WebUtility.UrlDecode(server), WebUtility.UrlDecode(login)));
                var connError = res.Item1;
                var info = res.Item2;

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
        public async Task<IActionResult> Post([FromBody]AccountDto account)
        {
            try
            {
                await _botAgent.AddAccount(new AccountKey(account.Server, account.Login), account.Password);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string login, string server)
        {
            try
            {
                await _botAgent.RemoveAccount(new AccountKey(server ?? "", login ?? ""));
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpPatch("[action]")]
        public async Task<IActionResult> UpdatePassword([FromBody] AccountDto account)
        {
            try
            {
                await _botAgent.ChangeAccountPassword(new AccountKey(account.Server, account.Login), account.Password);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Test(string login, string server, string password)
        {
            try
            {
                var testResult = string.IsNullOrWhiteSpace(password) ?
                    _botAgent.TestAccount(new AccountKey(server, login)) :
                    _botAgent.TestCreds(new AccountKey(server, login), password);

                return Ok(await testResult);
            }
            catch (BAException dsex)
            {
                _logger.LogError(dsex.Message);
                return BadRequest(dsex.ToBadResult());
            }
        }
    }
}
