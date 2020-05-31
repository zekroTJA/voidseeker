using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [TypeFilter(typeof(AuthorizationRequired))]
    public class UsersController : ControllerBase, IAuthorizedController
    {
        private AuthClaims authClaims;
        private readonly IDatabaseAccess database;
        private readonly IHasher hasher;

        public UsersController(IDatabaseAccess _database, IHasher _hasher)
        {
            database = _database;
            hasher = _hasher;
        }

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        public AuthClaims GetAuthClaims() => authClaims;

        // -------------------------------------------------------------------------
        // --- PUT /api/users ---

        [HttpPut]
        [AdminOnly]
        public async Task<ActionResult<UserModel>> Create([FromBody] UserCreateRequestModel user)
        {
            if (!user.Verify())
                return BadRequest(new ErrorModel(400, "invalid user model"));

            if (await database.GetUserByUserName(user.UserName) != null)
                return BadRequest(new ErrorModel(400, "username already taken"));

            user.AfterCreate();
            user.LastLogin = default;
            user.DisplayName = user.DisplayName.NullOrEmpty() ? user.UserName : user.DisplayName;
            user.PasswordHash = hasher.Create(user.Password);

            await database.Put(user);

            var resUser = new UserModel(user);

            return Created("user", resUser);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users ---

        [HttpGet]
        [AdminOnly]
        public async Task<ActionResult<PageModel<UserModel>>> Get(
            [FromQuery] int offset = 0, [FromQuery] int size = 20, [FromQuery] string filter = "")
        {
            var res = await database.SearchUsers(offset, size, filter);
            return Ok(new PageModel<UserModel>(res, offset));
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users/:ident ---

        [HttpGet("{ident}")]
        public async Task<ActionResult<UserModel>> GetUser([FromRoute] string ident)
        {
            UserModel user;

            if (ident.NullOrEmpty())
                return NotFound();

            if (Guid.TryParse(ident, out var uid))
                user = await database.Get<UserModel>(uid);
            else
                user = await database.GetUserByUserName(ident);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/users/@me ---

        [HttpGet("@me")]
        public Task<ActionResult<UserModel>> GetSelfUser() =>
            GetUser(authClaims.User?.Uid.ToString());

        // -------------------------------------------------------------------------
        // --- POST /api/users/:uid ---

        [HttpPost("{uid}")]
        [AdminOnly]
        public async Task<ActionResult<UserModel>> UpdateUser(
            [FromRoute] Guid? uid, [FromBody] UserCreateRequestModel newUser)
        {
            if (uid == null)
                return NotFound();

            var user = await database.Get<UserModel>(uid.Value);

            if (!newUser.UserName.NullOrEmpty())
                user.UserName = newUser.UserName;

            if (!newUser.DisplayName.NullOrEmpty())
                user.DisplayName = newUser.DisplayName;

            // If actually set to "", this will reset the entry for
            // the email address.
            if (newUser.EmailAddress != null)
                user.EmailAddress = newUser.EmailAddress;

            // Same for description
            if (newUser.Description != null)
                user.Description = newUser.Description;

            if (newUser.IsAdmin != null)
            {
                if (!authClaims.User.IsAdmin.Equals(true))
                    return BadRequest(new ErrorModel(400, "you need to be admin to change the admin state of a user"));

                user.IsAdmin = newUser.IsAdmin.Equals(true);
            }

            if (!newUser.Password.NullOrEmpty())
            {
                if (newUser.OldPassword.NullOrEmpty())
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
        public Task<ActionResult<UserModel>> UpdateSelfUser([FromBody] UserCreateRequestModel newUser) =>
            UpdateUser(authClaims.User?.Uid, newUser);

        // -------------------------------------------------------------------------
        // --- DELETE /api/users/:uid ---

        [HttpDelete("{uid}")]
        [AdminOnly]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid uid)
        {
            await database.Delete<UserModel>(uid);
            return Ok();
        }

        // -------------------------------------------------------------------------
        // --- DELETE /api/users/@me ---

        [HttpDelete("@me")]
        public Task<IActionResult> DeleteSelfUser() =>
            DeleteUser(authClaims.User.Uid);
    }
}