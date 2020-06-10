using System.Text.Json.Serialization;

namespace RESTAPI.Models.Responses
{
    /// <summary>
    /// Response model used to display
    /// general REST API errors.
    /// </summary>
    public class ErrorModel
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Initialize empty <see cref="ErrorModel"/>.
        /// </summary>
        public ErrorModel()
        {
        }

        /// <summary>
        /// Initialize new <see cref="ErrorModel"/> with passed
        /// error code and message.
        /// </summary>
        /// <param name="code">error code</param>
        /// <param name="message">error message</param>
        public ErrorModel(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
