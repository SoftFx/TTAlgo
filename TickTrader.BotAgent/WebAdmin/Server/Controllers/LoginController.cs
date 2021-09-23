using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly IAuthManager _authManager;
        private readonly ISecurityTokenProvider _tokenProvider;


        public LoginController(IAuthManager authManager, ISecurityTokenProvider tokenProvider)
        {
            _authManager = authManager;
            _tokenProvider = tokenProvider;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LoginDataDto loginData)
        {
            var identity = await _authManager.Login(loginData.Login, loginData.Password);
            if (identity == null)
            {
                return BadRequest(new BadRequestResultDto(ExceptionCodes.InvalidCredentials, "Invalid username or password"));
            }
            else
            {
                var encodedToken = _tokenProvider.CreateWebToken(identity, out var securityToken);

                return Json(new AuthDataDto { Token = encodedToken, Expires = securityToken.ValidTo, User = identity.Name });
            }
        }

        public class ExceptionCodes
        {
            public const int InvalidCredentials = 100;
        }
    }
}
