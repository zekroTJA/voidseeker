using Microsoft.AspNetCore.Mvc;
using RESTAPI.Authorization;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Filter;
using RESTAPI.Models;
using RESTAPI.Models.Responses;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    /// <summary>
    /// 
    /// TAGS CONTROLLER
    /// /api/tags
    /// 
    /// Provides endpoints for searching tags,
    /// getting tags metadata and deleting tags.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(401)]
    [TypeFilter(typeof(AuthorizationRequired))]
    public class TagsController : ControllerBase, IAuthorizedController
    {
        // --- Injected by DI ---------------------
        private readonly IDatabaseAccess database;
        // ----------------------------------------

        private AuthClaims authClaims;

        public AuthClaims GetAuthClaims() => authClaims;

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        public TagsController(IDatabaseAccess _database)
        {
            database = _database;
        }

        // -------------------------------------------------------------------------
        // --- GET /api/tags ---

        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<PageModel<TagModel>>> Get(
            [FromQuery] int offset = 0,
            [FromQuery] int size = 50,
            [FromQuery] string filter = "",
            [FromQuery] int fuzziness = -1)
        {
            var tags = await database.SearchTags(offset, size, filter, fuzziness);
            return Ok(new PageModel<TagModel>(tags, offset));
        }

        // -------------------------------------------------------------------------
        // --- GET /api/tags/:ident ---

        [HttpGet("{ident}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TagModel>> Get([FromRoute] string ident)
        {
            TagModel tag;

            if (ident.NullOrEmpty())
                return NotFound();

            if (Guid.TryParse(ident, out var uid))
                tag = await database.Get<TagModel>(uid);
            else
                tag = await database.GetTagByName(ident);

            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        // -------------------------------------------------------------------------
        // --- DELETE /api/tags/:uid ---

        [HttpDelete("{uid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [AdminOnly]
        public async Task<ActionResult> Delete([FromRoute] Guid uid)
        {
            var tag = await database.Get<TagModel>(uid);
            if (tag == null)
                return NotFound();

            await database.Delete<TagModel>(uid);

            return Ok();
        }
    }
}