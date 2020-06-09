using RESTAPI.Models;
using System;
using System.Collections.Generic;

namespace RESTAPI.Export
{
    /// <summary>
    /// Provides functionalities to initialize, destroy and get
    /// the status of <see cref="ExportWorker"/> instances.
    /// </summary>
    public interface IExportWorkerHandler
    {
        /// <summary>
        /// Initialize a <see cref="ExportWorker"/> with the
        /// passed userUid and list of images. The created
        /// worker is returned.
        /// </summary>
        /// <param name="userUid">users Uid</param>
        /// <param name="images">list of images</param>
        /// <returns></returns>
        ExportWorker InitializeWorker(Guid userUid, List<ImageModel> images);

        /// <summary>
        /// Destroy and cleanup a worker, when existent for
        /// the specified user.
        /// </summary>
        /// <param name="userUid">users Uid</param>
        void DestroyWorker(Guid userUid);

        /// <summary>
        /// Returns true if a worker is existent for the
        /// specified user Uid.
        /// </summary>
        /// <param name="userUid">user Uid</param>
        /// <returns></returns>
        bool HasWorker(Guid userUid);

        /// <summary>
        /// Tries to retrieve a worker from the handler for
        /// the specified user Uid. Returns true if a worker
        /// was found and the worker instance is set to the
        /// worker reference.
        /// </summary>
        /// <param name="userUid">users Uid</param>
        /// <param name="worker">worker reference</param>
        /// <returns></returns>
        bool TryGetWorker(Guid userUid, out ExportWorker worker);
    }
}
