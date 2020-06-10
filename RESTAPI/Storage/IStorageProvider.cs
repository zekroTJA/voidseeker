using System;
using System.IO;
using System.Threading.Tasks;

namespace RESTAPI.Storage
{
    /// <summary>
    /// Provides general functionalities to connect
    /// to an object storage.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Put an object to the object storage.
        /// </summary>
        /// <param name="bucket">bucket name</param>
        /// <param name="objectName">object name</param>
        /// <param name="data">data stream</param>
        /// <param name="size">size of object</param>
        /// <param name="contentType">content type / MIME type of the object</param>
        /// <param name="location">location of the storage server</param>
        /// <returns></returns>
        Task Put(string bucket, string objectName, Stream data, long size, string contentType, string location = null);

        /// <summary>
        /// Get an object from object storage. Returns a callback 
        /// <see cref="Action{Stream}"/> passing the object stream.
        /// </summary>
        /// <param name="bucket">bucket name</param>
        /// <param name="objectName">object name</param>
        /// <param name="cb">callback passing object data stream</param>
        /// <returns></returns>
        Task Get(string bucket, string objectName, Action<Stream> cb);

        /// <summary>
        /// Deletes an object from object storage.
        /// </summary>
        /// <param name="bucket">bucket name</param>
        /// <param name="objectName">object name</param>
        /// <returns></returns>
        Task Delete(string bucket, string objectName);
    }
}
