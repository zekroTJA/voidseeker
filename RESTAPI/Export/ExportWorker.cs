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
    public enum ExportWorkerStatus
    {
        FINISHED,
        INDEXING,
        COLLECTING,
        PACKING,
        CLEANUP,
    }

    public class ExportWorker
    {
        [JsonPropertyName("expires")]
        public DateTime Expires { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonPropertyName("status")]
        public ExportWorkerStatus Status { get; set; }

        [JsonIgnore]
        public string ArchiveFilePath { get; set; }


        private readonly IStorageProvider storage;
        private readonly string location;
        private readonly List<ImageModel> images;

        private CancellationTokenSource cancellation;

        public ExportWorker(IStorageProvider _storage, List<ImageModel> _images, string _location, TimeSpan expiresIn)
        {
            storage = _storage;
            location = _location;
            images = _images;

            Expires = DateTime.Now.Add(expiresIn);
        }

        public void Initialize()
        {
            cancellation = new CancellationTokenSource();
            Task.Run(Job, cancellation.Token);
        }

        public void Cancel()
        {
            if (cancellation != null)
                cancellation.Cancel();

            //try
            //{
                Directory.Delete(location, true);
            //} catch { }
        }

        private async Task Job()
        {
            var dataPath = Path.Combine(location, "data");
            var imagesPath = Path.Combine(dataPath, "images");

            Directory.CreateDirectory(imagesPath);

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

            Status = ExportWorkerStatus.PACKING;
            ArchiveFilePath = Path.Combine(location, $"archive.zip");
            ZipFile.CreateFromDirectory(dataPath, ArchiveFilePath);

            Status = ExportWorkerStatus.CLEANUP;
            Directory.Delete(dataPath, true);

            Status = ExportWorkerStatus.FINISHED;
            Finished = true;
        }
    }
}
