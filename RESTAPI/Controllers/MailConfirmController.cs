using Microsoft.AspNetCore.Mvc;
using RESTAPI.Cache;
using RESTAPI.Database;
using RESTAPI.Filter;
using RESTAPI.Models;
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
        // ----------------------------------------

        public MailConfirmController(IDatabaseAccess _database, ICacheProvider _cache)
        {
            database = _database;
            cache = _cache;
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
    }
}
