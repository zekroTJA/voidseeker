using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RESTAPI.Cache;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Filter;
using RESTAPI.Hashing;
using RESTAPI.Mailing;
using RESTAPI.Models;
using RESTAPI.Models.Requests;
using RESTAPI.Util;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    /// <summary>
    /// 
    /// MAILCONFIRM CONTROLLER
    /// /api/mailconfirm
    /// 
    /// Provides endpoints for confirming
    /// tokens in confirmation e-mails.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class MailConfirmController : ControllerBase
    {
        // --- Injected by DI ---------------------
        private readonly IDatabaseAccess database;
        private readonly ICacheProvider cache;
        private readonly IMailService mailService;
        private readonly IHasher hasher;

        // ----------------------------------------

        private readonly string publicAddress;

        public MailConfirmController(
            IDatabaseAccess _database, ICacheProvider _cache, IMailService _mailService, 
            IConfiguration _config, IHasher _hasher)
        {
            database = _database;
            cache = _cache;
            mailService = _mailService;
            hasher = _hasher;
            publicAddress = _config.GetValue<string>("WebServer:PublicURL", null);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/mailconfirm/confirmset ---

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(Nullable), 204)]
        [ProducesResponseType(typeof(Nullable), 401)]
        public async Task<ActionResult> ConfirmSet([FromQuery] string token)
        {
            var key = $"{Constants.MAIL_CONFIRM_CACHE_KEY}:{token}";

            var exists = cache.TryGet(key, out Guid userUid);
            if (!exists)
                return Unauthorized();

            var user = await database.Get<UserModel>(userUid);
            user.EmailConfirmStatus = EmailConfirmStatus.CONFIRMED;
            await database.Update(user);
            cache.Delete(key);

            return NoContent();
        }

        // -------------------------------------------------------------------------
        // --- POST /api/mailconfirm/passwordreset ---

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(Nullable), 204)]
        [ProducesResponseType(typeof(Nullable), 400)]
        public async Task<ActionResult> PasswordReset([FromBody] PasswordResetRequestModel pwReset)
        {
            var user = await database.GetUserByUserName(pwReset.UserName);
            if (user == null || user.EmailAddress.IsNullOrEmpty() || user.EmailAddress != pwReset.EmailAddress)
                return BadRequest();

            await SendPasswordResetMail(user);

            return NoContent();
        }

        // -------------------------------------------------------------------------
        // --- POST /api/mailconfirm/passwordresetconfirm ---

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(Nullable), 204)]
        [ProducesResponseType(typeof(Nullable), 401)]
        public async Task<ActionResult> PasswordResetConfirm([FromBody] PasswordConfirmRequestModel pwConfirm)
        {
            var key = $"{Constants.MAIL_PWRESET_CACHE_KEY}:{pwConfirm.Token}";

            if (!cache.TryGet<Guid>(key, out var userUid))
                return Unauthorized();

            var user = await database.Get<UserModel>(userUid);
            if (user == null)
                return Unauthorized();

            user.PasswordHash = hasher.Create(pwConfirm.NewPassword);
            await database.Update(user);
            cache.Delete(key);

            return NoContent();
        }

        // -------------------------------------------------------------------------
        // --- HELPERS ---

        /// <summary>
        /// TODO: Summary
        /// </summary>
        /// <param name="user">user model</param>
        /// <returns></returns>
        private async Task SendPasswordResetMail(UserModel user)
        {
            if (publicAddress.IsNullOrEmpty())
                return;

            var token = CryptoRandomUtil.GetBase64String(32);
            cache.Put($"{Constants.MAIL_PWRESET_CACHE_KEY}:{token}", user.Uid, TimeSpan.FromHours(1));

            var content =
                $"To reset your password, please follow the link below.\n" +
                $"\n" +
                $"Username:  {user.UserName}\n" +
                $"UID:  {user.Uid}\n" +
                $"Instance Host:  {publicAddress}\n" +
                $"\n" +
                $"Password Reset Link:\n" +
                $"{publicAddress}{Constants.MAIL_PWRESET_SUBDIR}?token={token}";

            await mailService.SendMailAsync("voidseeker", user.EmailAddress, "Password Reset | voidseeker", content, false);
        }
    }
}
