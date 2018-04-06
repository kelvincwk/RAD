using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Switchboard.Connectors
{
    /// <summary>
    /// Base class of cloud storage connector 
    /// </summary>
    public abstract class StorageConnectorBase
    {
        /// <summary>
        /// Authorise the app from the cloud API via the implemented connector
        /// To be implemented separately at derived class
        /// </summary>
        /// <returns>True or False</returns>
        protected abstract bool Authorise();
    }
}