using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Dto;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Controllers
{
    [Route("api/[controller]")]
    public class LoginController: Controller
    {
        private readonly IAuthManager _authManager;

        public LoginController(IAuthManager authManager)
        {
            _authManager = authManager;
        }

        [HttpPost]
        public IActionResult Post([FromBody]LoginDataDto loginData)
        {
            var identity = _authManager.Login(loginData.Login, loginData.Password);
            if (identity == null)
            {
                return BadRequest(new Models.BadRequestResult(ExceptionCodes.InvalidCredentials, "Invalid username or password"));
            }
            else
            {
                var jwt = _authManager.GetJwt(identity);
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Json(new AuthDataDto { Token = encodedJwt, Expires = jwt.ValidTo, User = identity.Name });
            }
        }
    }
}
