using Microsoft.AspNetCore.Mvc;
using RESTAPI.Authorization;
using RESTAPI.Database;
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

        [HttpPost]
        [AdminOnly]
        public async Task<ActionResult<UserModel>> Create([FromBody] UserCreateRequestModel user)
        {
            if (!user.Verify())
                return BadRequest(new ErrorModel(400, "invalid user model"));

            if (await database.GetUserByUserName(user.UserName) != null)
                return BadRequest(new ErrorModel(400, "username already taken"));

            user.Created = DateTime.Now;
            user.LastLogin = default;
            user.PasswordHash = hasher.Create(user.Password);

            await database.Put(user);

            var resUser = new UserModel(user);

            return Created("user", resUser);
        }

        [HttpGet]
        [AdminOnly]
        public async Task<ActionResult<PageModel<UserModel>>> Get(
            [FromQuery] int offset = 0, [FromQuery] int size = 20, [FromQuery] string filter = "")
        {
            var res = await database.SearchUsers(offset, size, filter);
            return Ok(new PageModel<UserModel>(res, offset));
        }

        [HttpGet("{ident}")]
        public async Task<ActionResult<UserModel>> GetUser(string ident)
        {
            UserModel user;

            if (ident == "@me")
                user = authClaims.User;
            else if (Guid.TryParse(ident, out var uid))
                user = await database.Get<UserModel>(uid);
            else
                user = await database.GetUserByUserName(ident);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}