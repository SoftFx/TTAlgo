using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using TickTrader.Algo.Server.PublicAPI.Adapter;
using TickTrader.BotAgent.WebAdmin.Server.Core.Auth;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> Post([FromBody] LoginDataDto loginData)
        {
            var authRes = await _authManager.Login(loginData.Login, loginData.Password);
            if (!authRes.Success)
            {
                return BadRequest(authRes.TemporarilyLocked
                    ? new BadRequestResultDto(ExceptionCodes.TemporarilyLocked, "Singin attempts limit exceeded. Try again later")
                    : new BadRequestResultDto(ExceptionCodes.InvalidCredentials, "Invalid username or password"));
            }
            else
            {
                if (authRes.Requires2FA)
                {
                    if (string.IsNullOrEmpty(loginData.SecretCode))
                        return BadRequest(new BadRequestResultDto(ExceptionCodes.Requires2FA, "2FA is required"));

                    authRes = await _authManager.Auth2FA(loginData.Login, loginData.SecretCode);
                    if (!authRes.Success)
                    {
                        return BadRequest(authRes.TemporarilyLocked
                            ? new BadRequestResultDto(ExceptionCodes.TemporarilyLocked, "Singin attempts limit exceeded. Try again later")
                            : new BadRequestResultDto(ExceptionCodes.Invalid2FA, "Invalid secret code"));
                    }
                }

                var identity = new ClaimsIdentity(new GenericIdentity(loginData.Login, "LoginToken"));
                var encodedToken = _tokenProvider.CreateWebToken(identity, out var securityToken);

                return Json(new AuthDataDto { Token = encodedToken, Expires = securityToken.ValidTo, User = identity.Name });
            }
        }


        public class ExceptionCodes
        {
            public const int InvalidCredentials = 100;
            public const int Requires2FA = 101;
            public const int TemporarilyLocked = 102;
            public const int Invalid2FA = 103;
        }
    }
}
