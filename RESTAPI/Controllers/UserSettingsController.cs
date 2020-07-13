using Microsoft.AspNetCore.Mvc;
using RESTAPI.Authorization;
using RESTAPI.Database;
using RESTAPI.Filter;
using RESTAPI.Models;
using RESTAPI.Models.Responses;
using System;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    /// <summary>
    /// 
    /// USERSETTINGS CONTROLLER
    /// /api/usersettings
    /// 
    /// Provides endpoints for getting and
    /// setting personal user preferences
    /// and settings.
    /// 
    /// </summary>
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Nullable), 401)]
    [TypeFilter(typeof(AuthorizationRequired))]
    [Route("api/[controller]")]
    [ApiController]
    public class UserSettingsController : ControllerBase, IAuthorizedController
    {
        // --- Injected by DI ---------------------
        private readonly IDatabaseAccess database;
        // ----------------------------------------

        private AuthClaims authClaims;

        public AuthClaims GetAuthClaims() => authClaims;

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        public UserSettingsController(IDatabaseAccess _database)
        {
            database = _database;
        }

        // -------------------------------------------------------------------------
        // --- GET /api/usersettings ---

        [HttpGet]
        [ProducesResponseType(typeof(UserSettingsModel), 200)]
        public ActionResult<UserSettingsModel> Get()
        {
            var userSettings = GetUserSettings();
            return Ok(userSettings);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/usersettings ---

        [HttpPost]
        [ProducesResponseType(typeof(UserSettingsModel), 200)]
        [ProducesResponseType(typeof(ErrorModel), 400)]
        public async Task<ActionResult<UserSettingsModel>> Post([FromBody] UserSettingsModel newUserSettings)
        {
            var userSettings = GetUserSettings();

            if (newUserSettings.TagBlacklist != null)
            {
                foreach (var t in newUserSettings.TagBlacklist)
                    if (!Regex.IsMatch(t, Constants.TAG_PATTERN))
                        return BadRequest(new ErrorModel(400, "invalid blacklist tag format"));

                userSettings.TagBlacklist =
                authClaims.User.TagBlacklist = newUserSettings.TagBlacklist;
            }

            await database.Update(authClaims.User);

            return Ok(userSettings);
        }

        // -------------------------------------------------------------------------
        // --- HELPERS ---

        private UserSettingsModel GetUserSettings()
        {
            var userSettings = new UserSettingsModel()
            {
                TagBlacklist = authClaims.User.TagBlacklist,
            };

            return userSettings;
        }
    }
}
