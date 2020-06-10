using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RESTAPI.Models
{
    /// <summary>
    /// Image Grade specification.
    /// </summary>
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

    /// <summary>
    /// Image metadata model.
    /// </summary>
    public class ImageModel : UniqueModel
    {
        [JsonPropertyName("owneruid")]
        public Guid OwnerUid { get; set; }

        [StringLength(32)]
        [JsonPropertyName("mimetype")]
        public string MimeType { get; set; }

        [StringLength(256)]
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("blobname")]
        public string BlobName { get; set; }

        [JsonPropertyName("bucket")]
        public string Bucket { get; set; }

        [StringLength(256)]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [StringLength(4096)]
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

        [JsonPropertyName("md5hash")]
        public string Md5Hash { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [StringLength(4096)]
        [JsonPropertyName("tagscombined")]
        public string TagsCombined { get; set; }

        [JsonPropertyName("tagsarray")]
        public string[] TagsArray
        {
            get => TagsCombined?.Split(" ") ?? new string[] { };
        }

        /// <summary>
        /// Initialize new empty <see cref="ImageModel"/> instance.
        /// </summary>
        public ImageModel() : base("images")
        {
        }

        /// <summary>
        /// Returns true when the passed tags are valid.
        /// Reason will be set to a string describing the
        /// reason why the tags are not valid, when they
        /// are not valid.
        /// </summary>
        /// <param name="reason">reason output</param>
        /// <returns></returns>
        public bool IsValidTags(out string reason)
        {
            var dict = new Dictionary<string, object>();
            foreach (var v in TagsArray)
            {
                if (!Regex.IsMatch(v, Constants.TAG_PATTERN))
                {
                    reason = "invalid tag format - must be lowercase and can only contain letters, numbers, underscores and minuses";
                    return false;
                }

                if (dict.ContainsKey(v))
                {
                    reason = "duplicate tags";
                    return false;
                }

                dict[v] = null;
            }

            reason = null;
            return true;
        }

        /// <summary>
        /// Transform tags to all lowercase.
        /// </summary>
        public void LowercaseTags()  =>
            TagsCombined = TagsCombined?.ToLower();
    }
}
