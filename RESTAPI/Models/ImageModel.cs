using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    public enum Grade
    {
        S,
        A,
        B,
        C,
        D,
        E,
        F
    }

    public class ImageModel : UniqueModel
    {
        [JsonPropertyName("owneruid")]
        public Guid OwnerUid { get; set; }

        [JsonPropertyName("mimetype")]
        public string MimeType { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("blobname")]
        public string BlobName { get; set; }

        [JsonPropertyName("bucket")]
        public string Bucket { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("explicit")]
        public bool? Explicit { get; set; }

        [JsonPropertyName("public")]
        public bool? Public { get; set; }

        [JsonPropertyName("grade")]
        public Grade? Grade { get; set; }

        [JsonPropertyName("tagscombined")]
        public string TagsCombined { get; set; }

        [JsonPropertyName("tagsarray")]
        public string[] TagsArray
        {
            get => TagsCombined?.Split(" ") ?? new string[] { };
        }

        public ImageModel() : base("images")
        {
        }
    }
}
