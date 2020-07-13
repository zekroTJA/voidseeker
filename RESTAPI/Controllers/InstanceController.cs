using Microsoft.AspNetCore.Mvc;
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
    /// INSTANCE CONTROLLER
    /// /api/instance
    /// 
    /// Provides endpoints for checking the
    /// instance status and initializing the
    /// instance on first startup.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class InstanceController : ControllerBase
    {
        // --- Injected by DI ---------------------
        private readonly IDatabaseAccess database;
        private readonly IHasher hasher;
        // ----------------------------------------

        public InstanceController(IDatabaseAccess _database, IHasher _hasher)
        {
            database = _database;
            hasher = _hasher;
        }

        // -------------------------------------------------------------------------
        // --- GET /api/instance/status ---

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(InstanceStatusModel), 200)]
        public async Task<InstanceStatusModel> Status()
        {
            var usersCount = await database.Count<UserModel>();
            var imageCount = await database.Count<ImageModel>();
            var version = GetType().Assembly.GetName().Version.ToString();

            return new InstanceStatusModel()
            {
                Initialized = usersCount > 0L,
                UsersCount = usersCount,
                ImagesCount = imageCount,
                Version = version,
            };
        }

        // -------------------------------------------------------------------------
        // --- POST /api/instance/initialize ---

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(UserModel), 201)]
        [ProducesResponseType(typeof(ErrorModel), 400)]
        [ProducesResponseType(typeof(Nullable), 401)]
        public async Task<ActionResult<UserModel>> Initialize([FromBody] UserCreateRequestModel user)
        {
            var usersCount = await database.Count<UserModel>();
            if (usersCount > 0L)
                return BadRequest(new ErrorModel(400, "already initialized"));

            if (!user.IsValidUsername())
                return BadRequest(new ErrorModel(400, "invalid username"));

            if (!user.IsValidPassword())
                return BadRequest(new ErrorModel(400, "invalid new password"));

            user.AfterCreate();
            user.LastLogin = default;
            user.IsAdmin = true;
            user.DisplayName = user.DisplayName.IsNullOrEmpty() ? user.UserName : user.DisplayName;
            user.PasswordHash = hasher.Create(user.Password);

            await database.Put(user);

            var resUser = new UserModel(user);

            return Created("user", resUser);
        }
    }
}