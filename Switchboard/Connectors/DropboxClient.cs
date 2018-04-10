using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Switchboard.Models;
using ETL = Microsoft.Practices.EnterpriseLibrary;

namespace Switchboard.Connectors
{
    /// <summary>
    /// Not implemented this time
    /// </summary>
    public class DropboxClient : Connectors.StorageConnectorBase, Connectors.IStorageConnector
    {
        protected override bool Authorise()
        {
            throw new NotImplementedException();
        }
        public void Add(string name, string resourceId)
        {
            throw new NotImplementedException();
        }

        public List<Item> GetChanges(string name, string resourceId)
        {
            throw new NotImplementedException();
        }

        public void Watch(string name, string sessionId)
        {
            throw new NotImplementedException();
        }
    }
}