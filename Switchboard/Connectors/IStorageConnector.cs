using Switchboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switchboard.Connectors
{
    /// <summary>
    /// Contract to define operations
    /// TODO: Methods are supposed to be async
    /// </summary>
    interface IStorageConnector
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sessionId"></param>
         void Watch(string name, string sessionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceId"></param>
        List<Item> GetChanges(string resourceId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceId"></param>
        void Add(string resourceId);
    }
}
