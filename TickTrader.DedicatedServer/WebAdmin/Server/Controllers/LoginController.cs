using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TickTrader.DedicatedServer.WebAdmin.Server.Models;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Controllers
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
        public IActionResult Post([FromBody]LoginModel loginModel)
        {
            var identity = _authManager.Login(loginModel.Login, loginModel.Password);
            if (identity == null)
            {
                return BadRequest("Invalid username or password");
            }
            else
            {
                var jwt = _authManager.GetJwt(identity);
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                return Json(new
                {
                    token = encodedJwt,
                    expires = jwt.ValidTo,
                    username = identity.Name
                });
            }
        }
    }
}
