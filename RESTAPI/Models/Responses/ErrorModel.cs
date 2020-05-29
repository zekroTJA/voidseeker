using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    public class ErrorModel
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ErrorModel()
        {
        }

        public ErrorModel(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
