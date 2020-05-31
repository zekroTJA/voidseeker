using System;
using System.IO;
using System.Threading.Tasks;

namespace RESTAPI.Storage
{
    public interface IStorageProvider
    {
        Task Put(string bucket, string objectName, Stream data, long size, string contentType, string location = null);
        Task Get(string bucket, string objectName, Action<Stream> cb);
    }
}
