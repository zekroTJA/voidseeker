using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio.Exceptions;
using RESTAPI.Authorization;
using RESTAPI.Database;
using RESTAPI.Extensions;
using RESTAPI.Filter;
using RESTAPI.Models;
using RESTAPI.Models.Responses;
using RESTAPI.Storage;
using RESTAPI.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RESTAPI.Controllers
{
    /// <summary>
    /// 
    /// IMAGES CONTROLLER
    /// /api/images
    /// 
    /// Provides endpoints for uploading and downloading
    /// images, searching imahes, getting and setting 
    /// metadata, deleting images and getting image 
    /// thumbnails.
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProxyAddress]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(401)]
    [TypeFilter(typeof(AuthorizationRequired))]
    public class ImagesController : ControllerBase, IAuthorizedController
    {
        // --- Injected by DI ---------------------
        private readonly IDatabaseAccess database;
        private readonly IStorageProvider storage;
        // ----------------------------------------
        
        private AuthClaims authClaims;

        public ImagesController(IDatabaseAccess _database, IStorageProvider _storage)
        {
            database = _database;
            storage = _storage;
        }

        public AuthClaims GetAuthClaims() => authClaims;

        public void SetAuthClaims(AuthClaims claims) =>
            authClaims = claims;

        // -------------------------------------------------------------------------
        // --- GET /api/images ---

        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<PageModel<ImageModel>>> Get(
            [FromQuery] int offset = 0,
            [FromQuery] int size = 50,
            [FromQuery] string filter = "",
            [FromQuery] string[] exclude = default,
            [FromQuery] bool includePublic = false,
            [FromQuery] bool includeExplicit = false,
            [FromQuery] string sortBy = "created",
            [FromQuery] bool ascending = false)
        {
            var res = await database.SearchImages(
                offset, size, filter, exclude, authClaims.UserUid, includePublic, includeExplicit, sortBy, ascending);

            return Ok(new PageModel<ImageModel>(res, offset));
        }

        // -------------------------------------------------------------------------
        // --- PUT /api/images ---

        [HttpPut]
        [RequestSizeLimit(100 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ImageModel>> PutImageData(IFormFile file)
        {
            if (file == null)
                return BadRequest(new ErrorModel(400, "no image attached"));

            if (!Constants.ALLOWED_CONTENT_TYPES.Contains(file.ContentType))
                return BadRequest(new ErrorModel(400, "invalid content type"));

            var image = new ImageModel();
            image.OwnerUid = authClaims.UserUid;
            image.BlobName = image.Uid.ToString();
            image.Bucket = Constants.IMAGE_STORAGE_BUCKET;
            image.Filename = file.FileName;
            image.MimeType = file.ContentType;
            image.Size = file.Length;
            image.Explicit = false;
            image.Public = false;

            var stream = file.OpenReadStream();

            image.Md5Hash = FileHashing.GetHash(stream);

            if (await database.GetImageByHash(image.Md5Hash, image.OwnerUid) != null)
                return BadRequest(new ErrorModel(400, "image already existent"));

            stream.Position = 0;
            var imageMeta = Image.FromStream(stream);
            image.Height = imageMeta.Height;
            image.Width = imageMeta.Width;

            stream.Position = 0;
            await storage.Put(image.Bucket, image.BlobName, stream, image.Size, image.MimeType);

            await database.Put(image);

            return Created("image", image);
        }



        // -------------------------------------------------------------------------
        // --- POST /api/images/:uid ---

        [HttpPost("{uid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ImageModel>> UpdateImage([FromRoute] Guid uid, [FromBody] ImageModel newImage)
        {
            bool tagsUpdated = false;

            var image = await database.Get<ImageModel>(uid);
            if (image.OwnerUid != authClaims.UserUid && !authClaims.User.IsAdmin.Equals(true))
                return NotFound();

            if (newImage.Description != null)
                image.Description = newImage.Description;

            if (newImage.Title != null)
                image.Title = newImage.Title;

            if (newImage.Explicit != null)
                image.Explicit = newImage.Explicit;

            if (newImage.Public != null)
                image.Public = newImage.Public;

            if (newImage.Grade != null)
                image.Grade = newImage.Grade;

            if (newImage.TagsCombined != null && !newImage.TagsArray.SequenceEqual(image.TagsArray))
            {
                image.TagsCombined = newImage.TagsCombined;
                image.LowercaseTags();
                if (!image.ValidateTags(out var reason))
                    return BadRequest(new ErrorModel(400, reason));
                tagsUpdated = true;
            }

            await database.Update(image);
            if (tagsUpdated)
                await SaveTags(image, authClaims.UserUid);

            return Ok(image);
        }

        // -------------------------------------------------------------------------
        // --- GET /api/images/:uid ---

        [HttpGet("{uid}")]
        [ResponseCache(Duration = 30 * 24 * 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> GetImage([FromRoute] Guid uid)
        {
            var image = await database.Get<ImageModel>(uid);
            if (image == null || (!image.Public.Equals(true) && image.OwnerUid != authClaims.UserUid && !authClaims.User.IsAdmin.Equals(true)))
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
        // --- GET /api/images/:uid/thumbnail ---

        [HttpGet("{uid}/thumbnail")]
        [ResponseCache(Duration = 30 * 24 * 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> GetImageThumbnail([FromRoute] Guid uid, [FromQuery] int size = 200)
        {
            byte[] imageDataArray;
            var fileBlobName = $"{uid}_{size}";

            try
            {
                var memStream = new MemoryStream();
                await storage.Get(Constants.THUMBNAIL_STORAGE_BUCKET, fileBlobName, (stream) =>
                {
                    stream.CopyTo(memStream);
                });
                imageDataArray = memStream.ToArray();
            } 
            catch (ObjectNotFoundException) 
            {
                var image = await database.Get<ImageModel>(uid);
                if (image == null || (!image.Public.Equals(true) && image.OwnerUid != authClaims.UserUid && !authClaims.User.IsAdmin.Equals(true)))
                    return NotFound();

                var oriImageMemStream = new MemoryStream();
                await storage.Get(image.Bucket, image.BlobName, (stream) =>
                {
                    stream.CopyTo(oriImageMemStream);
                });

                var oriImage = Image.FromStream(oriImageMemStream);
                var (width, height) = oriImage.ShrinkSize(size);
                var thumbImage = oriImage.GetThumbnailImage(width, height, () => false, IntPtr.Zero);

                var thumbMemStream = new MemoryStream();
                thumbImage.Save(thumbMemStream, ImageFormat.Png);
                imageDataArray = thumbMemStream.ToArray();
                await storage.Put(Constants.THUMBNAIL_STORAGE_BUCKET, fileBlobName, new MemoryStream(imageDataArray), imageDataArray.Length, "image/png");
            }

            return File(imageDataArray, "image/png");
        }

        // -------------------------------------------------------------------------
        // --- GET /api/images/:uid/info ---

        [HttpGet("{uid}/info")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ImageModel>> GetImageInfo([FromRoute] Guid uid)
        {
            var image = await database.Get<ImageModel>(uid);
            if (image == null || (!image.Public.Equals(true) && image.OwnerUid != authClaims.UserUid && !authClaims.User.IsAdmin.Equals(true)))
                return NotFound();

            return Ok(image);
        }

        // -------------------------------------------------------------------------
        // --- DELETE /api/images/:uid/info ---

        [HttpDelete("{uid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ImageModel>> DeleteImage([FromRoute] Guid uid)
        {
            var image = await database.Get<ImageModel>(uid);
            if (image == null || (image.OwnerUid != authClaims.UserUid && !authClaims.User.IsAdmin.Equals(true)))
                return NotFound();

            await database.Delete<ImageModel>(uid);
            await storage.Delete(image.Bucket, image.BlobName);

            return Ok();
        }

        // -------------------------------------------------------------------------
        // --- HELPER FUNCTIONS ---

        private string GetCacheImageKey(Guid uid) =>
            $"image:create:{uid.ToString()}";

        private async Task SaveTags(ImageModel image, Guid creator)
        {
            foreach (var t in image.TagsArray)
            {
                if ((await database.GetTagByName(t)) == null)
                {
                    await database.Put(new TagModel() { Name = t, CreatorUid = creator });
                }
            }
        }
    }
}