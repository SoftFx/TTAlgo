using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using TickTrader.Algo.Domain;
using System.Threading.Tasks;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ILogger<PackagesController> _logger;
        private readonly IAlgoServerLocal _algoServer;

        public AccountsController(IAlgoServerLocal algoServer, ILogger<PackagesController> logger)
        {
            _algoServer = algoServer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<AccountDto[]> Get()
        {
            var snapshot = await _algoServer.GetAccounts();
            return snapshot.Accounts.Select(a => a.ToDto()).ToArray();
        }

        [HttpGet("{server}/{login}/[action]")]
        public async Task<IActionResult> Info(string server, string login)
        {
            try
            {
                var accId = AccountId.Pack(WebUtility.UrlDecode(server), WebUtility.UrlDecode(login));
                var res = await _algoServer.GetAccountMetadata(new AccountMetadataRequest(accId));
                return Ok(res.ToDto());
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AccountDto account)
        {
            try
            {
                var request = new AddAccountRequest(account.Server, account.Login, account.Password);
                await _algoServer.AddAccount(request);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string login, string server)
        {
            try
            {
                var accId = AccountId.Pack(server, login);
                await _algoServer.RemoveAccount(new RemoveAccountRequest(accId));
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
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
                await _algoServer.ChangeAccount(request);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }

            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Test(string login, string server, string password)
        {
            try
            {
                var testResult = string.IsNullOrWhiteSpace(password) ?
                    _algoServer.TestAccount(new TestAccountRequest(AccountId.Pack(server, login))) :
                    _algoServer.TestCreds(new TestAccountCredsRequest(server, login, new AccountCreds(password)));

                return Ok(await testResult);
            }
            catch (AlgoException algoEx)
            {
                _logger.LogError(algoEx.Message);
                return BadRequest(algoEx.ToBadResult());
            }
        }
    }
}
