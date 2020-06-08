using Microsoft.AspNetCore.Mvc;
using RESTAPI.Authorization;
using RESTAPI.Database;
using RESTAPI.Export;
using RESTAPI.Filter;
using RESTAPI.Models;
using RESTAPI.Models.Responses;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(401)]
    [TypeFilter(typeof(AuthorizationRequired))]
    public class ExportController : ControllerBase, IAuthorizedController
    {
        private AuthClaims authClaims;
        private readonly IDatabaseAccess database;
        private readonly IExportWorkerHandler workerHandler;

        public AuthClaims GetAuthClaims() => authClaims;

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        public ExportController(IDatabaseAccess _database, IExportWorkerHandler _workerHandler)
        {
            database = _database;
            workerHandler = _workerHandler;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ExportWorker>> Initialize(
            [FromQuery] string filter = "",
            [FromQuery] string[] exclude = default,
            [FromQuery] bool includePublic = false,
            [FromQuery] bool includeExplicit = false)
        {
            if (workerHandler.HasWorker(authClaims.UserId))
                return BadRequest(new ErrorModel(400, "already initialized"));

            var count = await database.Count<ImageModel>();

            var res = await database.SearchImages(
                0, (int)count, filter, exclude, authClaims.UserId, includePublic, includeExplicit);

            var worker = workerHandler.InitializeWorker(authClaims.UserId, res);

            return Created("worker", worker);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult<ExportWorker> Status()
        {
            if (!workerHandler.TryGetWorker(authClaims.UserId, out var worker))
                return BadRequest(new ErrorModel(400, "not initialized"));

            return Ok(worker);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult Download()
        {
            if (!workerHandler.TryGetWorker(authClaims.UserId, out var worker))
                return BadRequest(new ErrorModel(400, "not initialized"));

            if (!worker.Finished)
                return BadRequest(new ErrorModel(400, "not finished"));

            var file = new FileStream(worker.ArchiveFilePath, FileMode.Open);
            return File(file, "application/zip", "image-archive.zip");
        }

        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult Remove()
        {
            if (!workerHandler.TryGetWorker(authClaims.UserId, out var worker))
                return BadRequest(new ErrorModel(400, "not initialized"));

            workerHandler.DestroyWorker(authClaims.UserId);
            return Ok();
        }
    }
}