using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Switchboard.Models;
using ETL = Microsoft.Practices.EnterpriseLibrary;
using ExactOnline.Client.Sdk;
using ExactOnline.Client.Models;
using ExactOnline.Client.OAuth;
using ExactOnline.Client.Models.CRM;
using ExactOnline.Client.Sdk.Controllers;
using ExactOnline.Client.Sdk.Helpers;

namespace Switchboard.Connectors
{
    public class ExactDocumentClient : Connectors.StorageConnectorBase, Connectors.IStorageConnector
    {
        #region "Private Members"
        private string _clientId;
        private string _clientSecret;
        private string _storageAuthenticatedCallbackRedirectURL;
        private ExactConnectorHelper _connector;
        private ExactOnlineClient _exactClient;
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExactDocumentClient()
        {
            ///TODO: These all can be set in the web.config file and be transformed in the publishing scripts if required
            _clientId = "4cb5d830 - 4192 - 4cbc - b245 - dff0c8ad18e2";
            _clientSecret = "mShp7gyt3Fz7";
            _storageAuthenticatedCallbackRedirectURL = "https://googledrivewatcher.azurewebsites.net/Home/ConnectDocumentCenter";
        }

        /// <summary>
        /// Authorise the app from the cloud API via the implemented connector
        /// </summary>
        protected override bool Authorise()
        {
            try {
                _connector = new ExactConnectorHelper(_clientId, _clientSecret, new Uri(_storageAuthenticatedCallbackRedirectURL));
                _exactClient = new ExactOnlineClient(_connector.EndPoint, _connector.GetAccessToken);

                return true;
            }
            catch(Exception ex)
            {
                ETL.Logging.Logger.Write(new ETL.Logging.LogEntry { Message = ex.Message });
                return false;
            }
        }
        /// <summary>
        /// Insert new document via the Exact online API
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resourceId"></param>
        public void Add(string name, string resourceId)
        {

            Switchboard.Notification.Instance.Notify(name, "Trying to authorize throu Exact Online and add the new document");

            ///If unauthorised it will be going to the OAuth2 process then revisited again in the callback (see HomeController.cs)
            if (!Authorise())
            {
                return;
            }
            try{
                //TODO: lookup the item to add using the resourceId
                var item = (Item)(HttpContext.Current.Session["syncItem"]);

                Document document = new Document
                {
                    Subject = item.FileName,
                    Body = item.DownloadURL,
                    Type = 55, //Miscellaneous
                    DocumentDate = DateTime.Now.Date
                };
                bool created = _exactClient.For<Document>().Insert(ref document);
                if (created)
                {
                    ///Chuck the files to be synced into the session
                    ///For demo purposes there will be only one hardcoded file to be added
                    /// TODO: encapsulate all session state access into the Session Information\User.cs
                    HttpContext.Current.Session.Remove("syncItem");
                }
            }
            catch (Exception ex)
            {
                ETL.Logging.Logger.Write(new ETL.Logging.LogEntry { Message = ex.Message });
            }            
        }

        public List<Item> GetChanges(string name, string pageToken)
        {
            throw new NotImplementedException();
        }

        public void Watch(string name, string sessionId)
        {
            throw new NotImplementedException();
        }
    }
}