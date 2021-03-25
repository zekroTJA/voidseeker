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
    [ProducesResponseType(typeof(Nullable), 401)]
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
        [ProducesResponseType(typeof(UserModel), 201)]
        [ProducesResponseType(typeof(ErrorModel), 400)]
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
        [ProducesResponseType(typeof(PageModel<UserModel>), 200)]
        public async Task<ActionResult<PageModel<UserModel>>> Get(
            [FromQuery] int offset = 0, [FromQuery] int size = 20, [FromQuery] string filter = "")
        {
            var res = await database.SearchUsers(offset, size, filter);
            return Ok(new PageModel<UserModel>(res, offset));
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users/:ident ---

        [HttpGet("{ident}")]
        [ProducesResponseType(typeof(UserDetailsModel), 200)]
        [ProducesResponseType(typeof(Nullable), 404)]
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
        [ProducesResponseType(typeof(UserDetailsModel), 200)]
        [ProducesResponseType(typeof(Nullable), 404)]
        public Task<ActionResult<UserDetailsModel>> GetSelfUser() =>
            GetUser(authClaims.UserUid.ToString());

        // -------------------------------------------------------------------------
        // --- POST /api/users/:uid ---

        [HttpPost("{uid}")]
        [AdminOnly]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(Nullable), 404)]
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
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(ErrorModel), 400)]
        [ProducesResponseType(typeof(Nullable), 404)]
        public Task<ActionResult<UserModel>> UpdateSelfUser([FromBody] UserCreateRequestModel newUser) =>
            UpdateUser(authClaims.User?.Uid, newUser);

        // -------------------------------------------------------------------------
        // --- POST /api/users/@me/resendconfirm ---

        [HttpPost("@me/resendconfirm")]
        [ProducesResponseType(typeof(Nullable), 204)]
        [ProducesResponseType(typeof(ErrorModel), 400)]
        public async Task<ActionResult> ResendConfirmationMail()
        {
            var user = authClaims.User;
            if (user.EmailConfirmStatus != EmailConfirmStatus.UNCONFIRMED)
                return BadRequest(new ErrorModel(400, "Mail is already confirmed or no mail was set."));

            await SendMailConfirm(user);

            return NoContent();
        }

        // -------------------------------------------------------------------------
        // --- DELETE /api/users/:uid ---

        [HttpDelete("{uid}")]
        [AdminOnly]
        [ProducesResponseType(typeof(Nullable), 204)]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid uid)
        {
            await database.Delete<UserModel>(uid);
            return NoContent();
        }

        // -------------------------------------------------------------------------
        // --- DELETE /api/users/@me ---

        [HttpDelete("@me")]
        [ProducesResponseType(typeof(Nullable), 204)]
        public Task<IActionResult> DeleteSelfUser() =>
            DeleteUser(authClaims.User.Uid);

        // -------------------------------------------------------------------------
        // --- HELPERS ---

        /// <summary>
        /// Generates a secure token which will be sent to the users
        /// e-mail address together with account and instance information.
        /// This token is also set to cache as key together with the users
        /// UID as value to re-identify this token.
        /// After that, teh value for <see cref="UserModel.EmailConfirmStatus"/>
        /// will be set to <see cref="EmailConfirmStatus.UNCONFIRMED"/>.
        /// 
        /// If the user has no valid e-mail address set, no mail will be sent
        /// and this function returns nothing without an error.
        /// </summary>
        /// <param name="user">user model</param>
        /// <returns></returns>
        private async Task SendMailConfirm(UserModel user)
        {
            if (publicAddress.IsNullOrEmpty())
                return;

            var token = CryptoRandomUtil.GetBase64String(32);
            cache.Put($"{Constants.MAIL_CONFIRM_CACHE_KEY}:{token}", user.Uid, TimeSpan.FromHours(1));

            var content =
                $"To confirm your mail address of your voidseeker account, please open the link below.\n" +
                $"\n" +
                $"Username:  {user.UserName}\n" +
                $"UID:  {user.Uid}\n" +
                $"Instance Host:  {publicAddress}\n" +
                $"\n" +
                $"Confirmation Link:\n" +
                $"{publicAddress}{Constants.MAIL_CONFIRM_SUBDIR}?token={token}";

            await mailService.SendMailAsync("voidseeker", user.EmailAddress, "E-Mail Confirmation | voidseeker", content, false);
            user.EmailConfirmStatus = EmailConfirmStatus.UNCONFIRMED;
        }
    }
}