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
    /// <summary>
    /// 
    /// AUTHORIZATION CONTROLLER
    /// /api/authorization
    /// 
    /// Provides endpoints for logging in and logging
    /// out using session cookies.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AuthController : ControllerBase
    {
        // --- Injected by DI -------------------------
        private readonly IAuthorization authorization;
        private readonly IDatabaseAccess database;
        private readonly IHasher hasher;
        // --------------------------------------------

        private static readonly TimeSpan DEFAULT_SESSION_EXPIRATION = TimeSpan.FromDays(1);
        private static readonly TimeSpan EXTENDED_SESSION_EXPIRATION = TimeSpan.FromDays(30);

        public AuthController(IAuthorization _authorization, IDatabaseAccess _database, IHasher _hasher)
        {
            authorization = _authorization;
            database = _database;
            hasher = _hasher;
        }

        // -------------------------------------------------------------------------
        // --- POST /api/auth/login ---

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserModel>> Login([FromBody] LoginModel login)
        {
            var user = await database.GetUserByUserName(login.Username);

            if (user == null)
                return Unauthorized();

            if (!hasher.Validate(login.Password, user.PasswordHash))
                return Unauthorized();

            user.LastLogin = DateTime.Now;
            await database.Update(user);

            var expies = login.Remember ? EXTENDED_SESSION_EXPIRATION : DEFAULT_SESSION_EXPIRATION;

            var claims = new AuthClaims()
            {
                UserUid = user.Uid,
                UserName = user.UserName,
            };

            var jwt = authorization.GetSessionKey(claims, expies);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.Add(expies),
                HttpOnly = true,
#if !DEBUG
                SameSite = SameSiteMode.Strict,
                Secure = true,
#endif
            };

            Response.Cookies.Append(Constants.SESSION_COOKIE_NAME, jwt, cookieOptions);

            return Ok(user);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/auth/logout ---

        [HttpPost("[action]")]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now,
                HttpOnly = true,
            };

            Response.Cookies.Append(Constants.SESSION_COOKIE_NAME, "", cookieOptions);

            return Ok();
        }
    }
}