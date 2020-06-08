using RESTAPI.Models;
using System;
using System.Collections.Generic;

namespace RESTAPI.Export
{
    public interface IExportWorkerHandler
    {
        ExportWorker InitializeWorker(Guid userUid, List<ImageModel> images);
        void DestroyWorker(Guid userUid);
        bool HasWorker(Guid userUid);
        bool TryGetWorker(Guid userUid, out ExportWorker worker);
    }
}
