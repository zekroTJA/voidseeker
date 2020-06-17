using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RESTAPI.Authorization;
using RESTAPI.Cache;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Filter;
using RESTAPI.Hashing;
using RESTAPI.Mailing;
using RESTAPI.Models;
using RESTAPI.Models.Requests;
using RESTAPI.Models.Responses;
using RESTAPI.Util;
using System;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    /// <summary>
    /// 
    /// USERS CONTROLLER
    /// /api/users
    /// 
    /// Provides endpoints searching, updating,
    /// creating and deleting users.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(401)]
    [TypeFilter(typeof(AuthorizationRequired))]
    public class UsersController : ControllerBase, IAuthorizedController
    {
        // --- Injected by DI ---------------------
        private readonly IDatabaseAccess database;
        private readonly IHasher hasher;
        private readonly IMailService mailService;
        private readonly ICacheProvider cache;
        // ----------------------------------------

        private AuthClaims authClaims;
        private readonly string publicAddress;

        public UsersController(
            IDatabaseAccess _database, IHasher _hasher, IMailService _mailService, 
            IConfiguration _config, ICacheProvider _cache)
        {
            database = _database;
            hasher = _hasher;
            mailService = _mailService;
            cache = _cache;
            publicAddress = _config.GetValue<string>("WebServer:PublicURL", null);
        }

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        public AuthClaims GetAuthClaims() => authClaims;

        // -------------------------------------------------------------------------
        // --- PUT /api/users ---

        [HttpPut]
        [AdminOnly]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserModel>> Create([FromBody] UserCreateRequestModel user)
        {

            // Validation is done by MVC request model.

            //if (!user.IsValidUsername())
            //    return BadRequest(new ErrorModel(400, "invalid username"));

            //if (!user.IsValidPassword())
            //    return BadRequest(new ErrorModel(400, "invalid new password"));

            if (await database.GetUserByUserName(user.UserName) != null)
                return BadRequest(new ErrorModel(400, "username already taken"));

            user.AfterCreate();
            user.LastLogin = default;
            user.DisplayName = user.DisplayName.IsNullOrEmpty() ? user.UserName : user.DisplayName;
            user.PasswordHash = hasher.Create(user.Password);
            // TODO: Replace with mail confirmation
            user.EmailAddress = null;

            await database.Put(user);

            var resUser = new UserModel(user);

            return Created("user", resUser);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users ---

        [HttpGet]
        [AdminOnly]
        [ProducesResponseType(200)]
        public async Task<ActionResult<PageModel<UserModel>>> Get(
            [FromQuery] int offset = 0, [FromQuery] int size = 20, [FromQuery] string filter = "")
        {
            var res = await database.SearchUsers(offset, size, filter);
            return Ok(new PageModel<UserModel>(res, offset));
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users/:ident ---

        [HttpGet("{ident}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDetailsModel>> GetUser([FromRoute] string ident)
        {
            UserModel user;

            if (ident.IsNullOrEmpty())
                return NotFound();

            if (Guid.TryParse(ident, out var uid))
                user = await database.Get<UserModel>(uid);
            else
                user = await database.GetUserByUserName(ident);

            if (user == null)
                return NotFound();

            if (!(authClaims.User?.IsAdmin).Equals(true))
            {
                user.EmailAddress = null;
                user.EmailConfirmStatus = EmailConfirmStatus.UNSET;
            }

            var detailedUser = new UserDetailsModel(user);
            detailedUser.ImagesCount = await database.Count<ImageModel>("ownerUid", user.Uid.ToString());

            return Ok(detailedUser);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users/@me ---

        [HttpGet("@me")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public Task<ActionResult<UserDetailsModel>> GetSelfUser() =>
            GetUser(authClaims.User?.Uid.ToString());

        // -------------------------------------------------------------------------
        // --- POST /api/users/:uid ---

        [HttpPost("{uid}")]
        [AdminOnly]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserModel>> UpdateUser(
            [FromRoute] Guid? uid, [FromBody] UserCreateRequestModel newUser)
        {
            if (uid == null)
                return NotFound();

            var user = await database.Get<UserModel>(uid.Value);

            // Update Username
            if (user.UserName != newUser.UserName && !newUser.UserName.IsNullOrEmpty())
            {
                if (await database.GetUserByUserName(user.UserName) != null)
                    return BadRequest(new ErrorModel(400, "username already taken"));

                user.UserName = newUser.UserName;
            }

            // Update Displayname
            if (!newUser.DisplayName.IsNullOrEmpty())
                user.DisplayName = newUser.DisplayName;

            // Update Email Address
            if (newUser.EmailAddress != null && newUser.EmailAddress != user.EmailAddress)
            {
                user.EmailAddress = newUser.EmailAddress;
                if (user.EmailAddress.Length > 0)
                    await SendMailConfirm(user);
                else
                    user.EmailConfirmStatus = EmailConfirmStatus.UNSET;
            }

            // Update Username
            if (newUser.Description != null)
                user.Description = newUser.Description;

            // Update Admin Status
            if (newUser.IsAdmin != null)
            {
                if (!authClaims.User.IsAdmin.Equals(true))
                    return BadRequest(new ErrorModel(400, "you need to be admin to change the admin state of a user"));

                user.IsAdmin = newUser.IsAdmin.Equals(true);
            }

            // Update Password
            if (!newUser.Password.IsNullOrEmpty())
            {
                if (!newUser.IsValidPassword())
                    return BadRequest(new ErrorModel(400, "invalid new password"));

                if (newUser.OldPassword.IsNullOrEmpty())
                    return BadRequest(new ErrorModel(400, "old password is required"));

                if (!hasher.Validate(newUser.OldPassword, user.PasswordHash))
                    return BadRequest(new ErrorModel(400, "invalid old password"));

                user.PasswordHash = hasher.Create(newUser.Password);
            }

            await database.Update(user);

            return Ok(user);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/users/@me ---

        [HttpPost("@me")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public Task<ActionResult<UserModel>> UpdateSelfUser([FromBody] UserCreateRequestModel newUser) =>
            UpdateUser(authClaims.User?.Uid, newUser);

        // -------------------------------------------------------------------------
        // --- DELETE /api/users/:uid ---

        [HttpDelete("{uid}")]
        [AdminOnly]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid uid)
        {
            await database.Delete<UserModel>(uid);
            return Ok();
        }

        // -------------------------------------------------------------------------
        // --- DELETE /api/users/@me ---

        [HttpDelete("@me")]
        [ProducesResponseType(200)]
        public Task<IActionResult> DeleteSelfUser() =>
            DeleteUser(authClaims.User.Uid);

        // -------------------------------------------------------------------------
        // --- HELPERS ---

        private async Task SendMailConfirm(UserModel user)
        {
            if (publicAddress.IsNullOrEmpty())
                return;

            var token = CryptoRandomUtil.GetBase64String(32)
                .Replace('=', '_')
                .Replace('+', '-');
            cache.Put($"{Constants.MAIL_CONFIRM_CACHE_KEY}:{token}", user.Uid, TimeSpan.FromHours(1));

            var content =
                $"To confirm your mail address of your voidseeker account, please open the link below.\n" +
                $"\n" +
                $"Username:      {user.UserName}\n" +
                $"UID:           {user.Uid}\n" +
                $"Instance Host: {publicAddress}\n" +
                $"\n" +
                $"Confirmation Link:\n" +
                $"{publicAddress}{Constants.MAIL_CONFIRM_SUBDIR}?token={token}";

            await mailService.SendMailAsync("voidseeker", user.EmailAddress, "E-Mail Confirmation | voidseeker", content, false);
            user.EmailConfirmStatus = EmailConfirmStatus.UNCONFIRMED;
        }
    }
}