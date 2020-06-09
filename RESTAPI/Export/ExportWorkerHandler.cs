using Microsoft.Extensions.Configuration;
using RESTAPI.Models;
using RESTAPI.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace RESTAPI.Export
{
    /// <summary>
    /// Implementation of <see cref="IExportWorkerHandler"/>.
    /// </summary>
    public class ExportWorkerHandler : IExportWorkerHandler
    {
        private readonly IStorageProvider storage;

        private readonly string bundlingLocation;
        private readonly TimeSpan expiration;

        private readonly Timer cleanupTimer;
        private readonly ConcurrentDictionary<Guid, ExportWorker> workers;

        /// <summary>
        /// Creates a new instance of <see cref="ExportWorkerHandler"/> with the
        /// specified <see cref="IStorageProvider"/> and <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="_storage"></param>
        /// <param name="configuration"></param>
        public ExportWorkerHandler(IStorageProvider _storage, IConfiguration configuration)
        {
            workers = new ConcurrentDictionary<Guid, ExportWorker>();
            storage = _storage;

            bundlingLocation = configuration.GetValue("Export:BundlingLocation", "./tmp/export");
            expiration = configuration.GetValue("Export:Expiration", TimeSpan.FromHours(1));

            cleanupTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            cleanupTimer.Elapsed += OnCleanup;
            cleanupTimer.Start();
        }

        public ExportWorker InitializeWorker(Guid userUid, List<ImageModel> images)
        {
            var worker = new ExportWorker(
                storage, images, Path.Combine(bundlingLocation, userUid.ToString()), expiration);

            if (!workers.TryAdd(userUid, worker))
                return null;

            worker.Initialize();

            return worker;
        }

        public void DestroyWorker(Guid userUid)
        {

            if (!TryGetWorker(userUid, out var worker))
                return;

            worker.Cancel();
            while (!workers.TryRemove(userUid, out _)) ;
        }

        public bool HasWorker(Guid userUid) =>
            workers.ContainsKey(userUid);

        public bool TryGetWorker(Guid userUid, out ExportWorker worker)
        {
            if (!HasWorker(userUid))
            {
                worker = null;
                return false;
            }

            while (!workers.TryGetValue(userUid, out worker)) ;

            return true;
        }

        /// <summary>
        /// Executes on cleanup timer elapse and destorys all
        /// workers which are expired.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void OnCleanup(object o, EventArgs args)
        {
            var now = DateTime.Now;

            foreach (var kv in workers)
            {
                if (kv.Value.Expires <= now)
                {
                    DestroyWorker(kv.Key);
                }
            }
        }
    }
}
