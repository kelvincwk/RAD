using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Switchboard.Entities
{
    /// <summary>
    /// Entity to be stored into session
    /// TODO: This needs to be decorated with Serializable so it can be stored into session state server
    /// </summary>
    [Serializable]
    public class User
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CloudProvider { get; set; }
        public string ResourceId { get; set; }
        public string LastSyncedPageToken { get; set; }
    }
}