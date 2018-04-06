using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Switchboard.Models
{
    [Serializable]
    public class Item
    {
        /// <summary>
        /// The file name
        /// </summary>
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// The unique file Id
        /// </summary>
        [JsonProperty(PropertyName = "resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// The content download URL
        /// </summary>
        [JsonProperty(PropertyName = "downloadURL")]
        public string DownloadURL { get; set; }

        /// <summary>
        /// The file type
        /// </summary>
        [JsonProperty(PropertyName = "mimeType")]
        public string MIMEType { get; set; }
    }
}