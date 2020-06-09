using RESTAPI.Models;
using RESTAPI.Storage;
using RESTAPI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RESTAPI.Export
{
    /// <summary>
    /// <see cref="ExportWorker"/> progress status definitions.
    /// </summary>
    public enum ExportWorkerStatus
    {
        FINISHED,
        INDEXING,
        COLLECTING,
        PACKING,
        CLEANUP,
        ERRORED,
    }

    /// <summary>
    /// Provides functionalities to collect image files using
    /// the specified <see cref="IStorageProvider"/> and sving
    /// them to the specified location. After that, these are
    /// bundled to a zip and provided for download.
    /// 
    /// This class also provides JSOn-serializable information
    /// about the worker status.
    /// </summary>
    public class ExportWorker
    {
        [JsonPropertyName("expires")]
        public DateTime Expires { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonPropertyName("status")]
        public ExportWorkerStatus Status { get; set; }

        [JsonPropertyName("exception")]
        public ExportWorkerExceptionModel Exception { get; set; }

        [JsonIgnore]
        public string ArchiveFilePath { get; set; }


        private readonly IStorageProvider storage;
        private readonly string location;
        private readonly List<ImageModel> images;

        private CancellationTokenSource cancellation;

        /// <summary>
        /// Create new <see cref="ExportWorker"/> instance.
        /// </summary>
        /// <param name="_storage">storage provider</param>
        /// <param name="_images">image list</param>
        /// <param name="_location">bundling lcoation</param>
        /// <param name="expiresIn">life span</param>
        public ExportWorker(IStorageProvider _storage, List<ImageModel> _images, string _location, TimeSpan expiresIn)
        {
            storage = _storage;
            location = _location;
            images = _images;

            Expires = DateTime.Now.Add(expiresIn);
        }

        /// <summary>
        /// Starts the collection worker task in a new thread.
        /// </summary>
        public void Initialize()
        {
            cancellation = new CancellationTokenSource();
            Task.Run(CatchException(Job), cancellation.Token);
        }

        /// <summary>
        /// Cancels the worker task and removes temporary
        /// files in the bundling location.
        /// </summary>
        public void Cancel()
        {
            if (cancellation != null)
                cancellation.Cancel();

            Directory.Delete(location, true);
        }

        /// <summary>
        /// Wrapper function for <see cref="Job"/> which catches
        /// exceptions, sets the status of the worker to 
        /// <see cref="ExportWorkerStatus.ERRORED"/> and sets the
        /// thrown exception as <see cref="Exception"/>.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        private Func<Task> CatchException(Func<Task> job)
        {
            return async () =>
            {
                try { await job(); }
                catch (Exception e)
                {
                    Exception = new ExportWorkerExceptionModel(e);
                    Status = ExportWorkerStatus.ERRORED;
                }
            };
        }

        /// <summary>
        /// Collection task.
        /// </summary>
        /// <returns></returns>
        private async Task Job()
        {
            var dataPath = Path.Combine(location, "data");
            var imagesPath = Path.Combine(dataPath, "images");

            // -------------------------------------------------------------------------------------------
            // --- Create temporary bundling directory.

            Directory.CreateDirectory(imagesPath);

            // -------------------------------------------------------------------------------------------
            // --- Create image index file and write to temp lcoation.

            Status = ExportWorkerStatus.INDEXING;
            var indexData = JsonSerializer.Serialize(images, new JsonSerializerOptions
            {
                WriteIndented = true,
            });

            var indexFilePath = Path.Combine(dataPath, "index.json");
            using (var indexFile = new StreamWriter(indexFilePath))
            {
                indexFile.Write(indexData);
            }

            // -------------------------------------------------------------------------------------------
            // --- Collectlisted images and copy them to the sepcified temp location.

            Status = ExportWorkerStatus.COLLECTING;
            foreach (var img in images)
            {
                var fileName = $"{img.Uid}.{FileUtil.GetFileExtensionByContentType(img.MimeType)}";
                var filePath = Path.Combine(imagesPath, fileName);

                using (var file = new FileStream(filePath, FileMode.Create))
                {
                    await storage.Get(img.Bucket, img.Uid.ToString(), (stream) =>
                    {
                        stream.CopyTo(file);
                    });
                }
            }

            // -------------------------------------------------------------------------------------------
            // --- Create zip bundle with index file and image files.

            Status = ExportWorkerStatus.PACKING;
            ArchiveFilePath = Path.Combine(location, $"archive.zip");
            ZipFile.CreateFromDirectory(dataPath, ArchiveFilePath);

            // -------------------------------------------------------------------------------------------
            // --- Delete temp bundling directory.

            Status = ExportWorkerStatus.CLEANUP;
            Directory.Delete(dataPath, true);

            // -------------------------------------------------------------------------------------------
            // --- Finish.

            Status = ExportWorkerStatus.FINISHED;
            Finished = true;
        }
    }
}
