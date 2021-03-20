using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RESTAPI.Authorization;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Filter;
using RESTAPI.Hashing;
using RESTAPI.Models;
using RESTAPI.Models.Requests;
using RESTAPI.Models.Responses;
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
        private readonly IRefreshTokenHandler refreshTokenHandler;
        private readonly IAccessTokenHandler accessTokenHandler;
        private readonly IDatabaseAccess database;
        private readonly IHasher hasher;
        // --------------------------------------------

        private readonly bool bypassSecureCookies;

        private static readonly TimeSpan DEFAULT_SESSION_EXPIRATION = TimeSpan.FromDays(1);
        private static readonly TimeSpan EXTENDED_SESSION_EXPIRATION = TimeSpan.FromDays(30);

        public AuthController(
            IRefreshTokenHandler _refreshTokenHandler,
            IAccessTokenHandler _accessTokenHandler,
            IDatabaseAccess _database, IHasher _hasher, 
            IConfiguration configuration)
        {
            refreshTokenHandler = _refreshTokenHandler;
            accessTokenHandler = _accessTokenHandler;
            database = _database;
            hasher = _hasher;

            bypassSecureCookies = configuration.GetValue("WebServer:Auth:BypassSecureCookies", false);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/auth/login ---

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(LoginResponseModel), 200)]
        [ProducesResponseType(typeof(Nullable), 401)]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginModel login)
        {
            var user = await database.GetUserByUserName(login.Username);

            if (user == null)
                return Unauthorized();

            if (!hasher.Validate(login.Password, user.PasswordHash))
                return Unauthorized();

            user.LastLogin = DateTime.Now;
            await database.Update(user);

            var expires = login.Remember ? EXTENDED_SESSION_EXPIRATION : DEFAULT_SESSION_EXPIRATION;

            var claims = new AuthClaims()
            {
                UserUid = user.Uid,
                UserName = user.UserName,
            };

            var refreshToken = await refreshTokenHandler.GenerateAsync(claims, expires);
            var accessToken = accessTokenHandler.Generate(claims);

            var cookieOptions = new CookieOptions
            {
                Expires = refreshToken.Deadline,
                HttpOnly = true,
#if !DEBUG
                SameSite = SameSiteMode.Strict,
                Secure = !bypassSecureCookies,
#endif
            };

            Response.Cookies.Append(Constants.REFRESH_TOKEN_COOKIE, refreshToken.Token, cookieOptions);

            return Ok(new LoginResponseModel(user, accessToken));
        }

        // -------------------------------------------------------------------------
        // --- GET /api/auth/accesstoken ---

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(DeadlinedToken), 200)]
        [ProducesResponseType(typeof(Nullable), 401)]
        public async Task<ActionResult<DeadlinedToken>> AccessToken()
        {
            if (!HttpContext.Request.ExtractRefreshToken(out var refreshToken))
                return Unauthorized();

            var identity = await refreshTokenHandler.ValidateAndRestoreAsync<AuthClaims>(refreshToken);
            if (identity == null)
                return Unauthorized();

            return accessTokenHandler.Generate(identity);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/auth/logout ---

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(Nullable), 204)]
        public async Task<IActionResult> Logout()
        {
            if (!HttpContext.Request.ExtractAccessToken(out var accessToken)
                || !accessTokenHandler.ValidateAndRestore<AuthClaims>(accessToken, out var identity))
                return Unauthorized(Constants.INVALID_ACCESS_TOKEN);

            var refreshToken = await database.GetRefreshTokenByUserUid(identity.UserUid);
            if (refreshToken != null)
                await database.Delete<RefreshTokenModel>(refreshToken.Uid);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now,
                HttpOnly = true,
            };

            Response.Cookies.Append(Constants.REFRESH_TOKEN_COOKIE, "", cookieOptions);

            return NoContent();
        }
    }
}