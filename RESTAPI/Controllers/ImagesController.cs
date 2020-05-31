using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTAPI.Authorization;
using RESTAPI.Cache;
using RESTAPI.Database;
using RESTAPI.Filter;
using RESTAPI.Models;
using RESTAPI.Models.Responses;
using RESTAPI.Storage;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [TypeFilter(typeof(AuthorizationRequired))]
    public class ImagesController : ControllerBase, IAuthorizedController
    {
        private AuthClaims authClaims;
        private readonly IDatabaseAccess database;
        private readonly IStorageProvider storage;
        private readonly ICacheProvider cache;

        public ImagesController(IDatabaseAccess _database, IStorageProvider _storage, ICacheProvider _cache)
        {
            database = _database;
            storage = _storage;
            cache = _cache;
        }

        public AuthClaims GetAuthClaims() => authClaims;

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        // -------------------------------------------------------------------------
        // --- GET /api/images ---

        [HttpGet]
        public ActionResult<PageModel<ImageModel>> Get(
            [FromQuery] int offset = 0,
            [FromQuery] int size = 50,
            [FromQuery] string filter = "")
        {
            return Ok();
        }

        // -------------------------------------------------------------------------
        // --- PUT /api/images ---

        [HttpPut]
        public ActionResult<CompletionWrapperModel<ImageModel>> Put([FromBody] ImageModel image)
        {
            image.AfterCreate();
            image.OwnerUid = authClaims.UserId;
            image.BlobName = null;
            image.Bucket = null;
            image.Filename = null;
            image.MimeType = null;
            image.Size = -1;

            var deadlineUntil = TimeSpan.FromMinutes(15);
            cache.Put(GetCacheImageKey(image.Uid), image, deadlineUntil);

            return Created("image-placeholder", new CompletionWrapperModel<ImageModel>()
            {
                Uid = image.Uid,
                Initialized = DateTime.Now,
                Deadline = DateTime.Now.Add(deadlineUntil),
                CompletionResource = $"/api/images/{image.Uid.ToString()}",
                Data = image,
            });
        }

        // -------------------------------------------------------------------------
        // --- PUT /api/images/:uid ---

        [HttpPut("{uid}")]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImageModel>> PutImageData(IFormFile file, [FromRoute] Guid uid)
        {
            if (file == null)
                return BadRequest(new ErrorModel(400, "no image attached"));

            if (!cache.Contains(GetCacheImageKey(uid)) || !cache.TryGet<ImageModel>(GetCacheImageKey(uid), out var image))
                return NotFound(new ErrorModel(404, "not found or deadline has expired"));

            if (image.OwnerUid != authClaims.UserId)
                return NotFound(new ErrorModel(404, "not found or deadline has expired"));

            image.BlobName = uid.ToString();
            image.Bucket = Constants.IMAGE_STORAGE_BUCKET;
            image.Filename = file.FileName;
            image.MimeType = file.ContentType;
            image.Size = file.Length;

            var stream = file.OpenReadStream();
            await storage.Put(image.Bucket, image.BlobName, stream, image.Size, image.MimeType);

            await database.Put(image);

            cache.Delete(GetCacheImageKey(uid));

            return Ok(image);
        }

        // -------------------------------------------------------------------------
        // --- POST /api/images/:uid ---

        [HttpPost("{uid}")]
        public async Task<ActionResult<ImageModel>> UpdateImage([FromRoute] Guid uid, [FromBody] ImageModel newImage)
        {
            var image = await database.Get<ImageModel>(uid);
            if (image.OwnerUid != authClaims.UserId && !authClaims.User.IsAdmin.Equals(true))
                return NotFound();

            if (newImage.Description != null)
                image.Description = newImage.Description;

            if (newImage.Title != null)
                image.Title = newImage.Title;

            if (newImage.Explicit != null)
                image.Explicit = newImage.Explicit;

            if (newImage.Public != null)
                image.Explicit = newImage.Public;

            if (newImage.Grade != null)
                image.Grade = newImage.Grade;

            if (newImage.TagsCombined != null)
                image.TagsCombined = newImage.TagsCombined;

            await database.Update(image);

            return Ok(image);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/images/:uid ---

        [HttpGet("{uid}")]
        [ResponseCache(Duration = 30 * 24 * 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<ActionResult> GetImage([FromRoute] Guid uid)
        {
            var image = await database.Get<ImageModel>(uid);
            if (image == null || (!image.Public.Equals(true) && image.OwnerUid != authClaims.UserId && !authClaims.User.IsAdmin.Equals(true)))
                return NotFound();

            var data = new byte[(int)image.Size];
            var memStream = new MemoryStream(data);

            await storage.Get(image.Bucket, image.BlobName, (stream) =>
            {
                stream.CopyTo(memStream);
            });

            return File(data, image.MimeType);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/images/:uid/info ---

        [HttpGet("{uid}/info")]
        public async Task<ActionResult<ImageModel>> GetImageInfo([FromRoute] Guid uid)
        {
            var image = await database.Get<ImageModel>(uid);
            if (image == null || (!image.Public.Equals(true) && image.OwnerUid != authClaims.UserId && !authClaims.User.IsAdmin.Equals(true)))
                return NotFound();

            return Ok(image);
        }

        private string GetCacheImageKey(Guid uid) =>
            $"image:create:{uid.ToString()}";
    }
}