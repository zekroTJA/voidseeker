using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTAPI.Authorization;
using RESTAPI.Database;
using RESTAPI.Filter;
using RESTAPI.Hashing;
using RESTAPI.Models;
using RESTAPI.Models.Requests;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthorization authorization;
        private readonly IDatabaseAccess database;
        private readonly IHasher hasher;

        public AuthController(IAuthorization _authorization, IDatabaseAccess _database, IHasher _hasher)
        {
            authorization = _authorization;
            database = _database;
            hasher = _hasher;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<UserModel>> Login([FromBody] LoginModel login)
        {
            var user = await database.GetUserByUserName(login.Username);

            if (user == null)
                return Unauthorized();

            if (!hasher.Validate(login.Password, user.PasswordHash))
                return Unauthorized();

            user.LastLogin = DateTime.Now;
            await database.Update(user);

            var claims = new AuthClaims()
            {
                UserId = user.UID,
                UserName = user.UserName,
            };

            var jwt = authorization.GetSessionKey(claims);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.Add(authorization.GetExpiration()),
                HttpOnly = true,
#if !DEBUG
                SameSite = SameSiteMode.Strict,
                Secure = true,
#endif
            };

            Response.Cookies.Append(Constants.SESSION_COOKIE_NAME, jwt, cookieOptions);

            return Ok(user);
        }

    }
}