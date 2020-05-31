using Microsoft.Extensions.Configuration;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RESTAPI.Storage
{
    public class MinioStorageProvider : IStorageProvider
    {
        private readonly MinioClient client;
        private readonly string defaultLocation;

        public MinioStorageProvider(IConfiguration configuration)
        {
            var section = configuration.GetSection("Storage:Minio");

            defaultLocation = section.GetValue<string>("DefaultLocation");

            client = new MinioClient(
                section.GetValue<string>("Endpoint"),
                section.GetValue<string>("AccessKey"),
                section.GetValue<string>("SecretKey"),
                defaultLocation ?? "");

            if (section.GetValue("SSLEnabled", false))
                client.WithSSL();
        }

        public async Task Put(
            string bucket, string objectName, Stream data, long size, string contentType, string location = null)
        {
            location = location ?? defaultLocation;

            await CreateBucketIfNotExistent(bucket, location);
            await client.PutObjectAsync(bucket, objectName, data, size, contentType);
        }

        public Task Get(string bucket, string objectName, Action<Stream> cb) =>
            client.GetObjectAsync(bucket, objectName, cb);

        private async Task CreateBucketIfNotExistent(string bucket, string location = null)
        {
            bool exists = false;

            try
            {
                exists = await client.BucketExistsAsync(bucket);
            } 
            catch (Exception e) 
            {
                Console.WriteLine(e);
            }
                
            if (!exists)
                await client.MakeBucketAsync(bucket, location ?? "is-east-1");
        }
    }
}
