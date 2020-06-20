using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RESTAPI.Cache;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Filter;
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
        // ----------------------------------------

        private readonly string publicAddress;

        public MailConfirmController(
            IDatabaseAccess _database, ICacheProvider _cache, IMailService _mailService, IConfiguration _config)
        {
            database = _database;
            cache = _cache;
            mailService = _mailService;
            publicAddress = _config.GetValue<string>("WebServer:PublicURL", null);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/mailconfirm/confirmset ---

        [HttpPost("[action]")]
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

            return Ok();
        }

        // -------------------------------------------------------------------------
        // --- POST /api/mailconfirm/passwordreset ---

        [HttpPost("[action]")]
        public async Task<ActionResult> PasswordReset([FromBody] PasswordResetRequestModel pwReset)
        {
            var user = await database.GetUserByUserName(pwReset.UserName);
            if (user.EmailAddress.IsNullOrEmpty() || user.EmailAddress != pwReset.EmailAddress)
                return BadRequest();

            await SendPasswordResetMail(user);

            return Ok();
        }

        // -------------------------------------------------------------------------
        // --- HELPERS ---

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
