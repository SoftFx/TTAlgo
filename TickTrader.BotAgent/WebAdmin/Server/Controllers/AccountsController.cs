using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using TickTrader.Algo.Domain;
using System.Threading.Tasks;
using TickTrader.Algo.Domain.ServerControl;

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
                var accId = AccountId.Pack(WebUtility.UrlDecode(server), WebUtility.UrlDecode(login));
                var res = await _botAgent.GetAccountMetadata(accId);
                var connError = res.Item1;
                var info = res.Item2;

                if (connError.IsOk)
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
                var request = new AddAccountRequest(account.Server, account.Login, account.Password);
                await _botAgent.AddAccount(request);
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
                var accId = AccountId.Pack(server, login);
                await _botAgent.RemoveAccount(new RemoveAccountRequest(accId));
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
                var accId = AccountId.Pack(account.Server, account.Login);
                var request = new ChangeAccountRequest(accId, new AccountCreds(account.Password));
                await _botAgent.ChangeAccount(request);
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
                    _botAgent.TestAccount(new TestAccountRequest(AccountId.Pack(server, login))) :
                    _botAgent.TestCreds(new TestAccountCredsRequest(server, login, new AccountCreds(password)));

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
