using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Export
{
    public class ExportWorkerExceptionModel
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        public ExportWorkerExceptionModel()
        {
        }

        public ExportWorkerExceptionModel(Exception e)
        {
            Message = e.Message;
            Source = e.Source;
        }
    }
}
