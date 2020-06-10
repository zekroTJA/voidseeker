using System;
using System.Text.Json.Serialization;

namespace RESTAPI.Models
{
    /// <summary>
    /// Base class defining a database and
    /// request body model containing an Uid,
    /// an Index and a Created date.
    /// </summary>
    public class UniqueModel
    {
        [JsonPropertyName("uid")]
        public Guid Uid { get; set; }

        [JsonPropertyName("index")]
        public string Index { get; private set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Create new <see cref="UniqueModel"/> with
        /// generated Uid and Created date and passed
        /// Index string.
        /// </summary>
        /// <param name="index"></param>
        public UniqueModel(string index)
        {
            Uid = Guid.NewGuid();
            Created = DateTime.Now;
            Index = index;
        }

        /// <summary>
        /// Re-generate and set Uid and Created date.
        /// </summary>
        /// <returns></returns>
        public UniqueModel AfterCreate()
        {
            Uid = Guid.NewGuid();
            Created = DateTime.Now;

            return this;
        }
    }
}
