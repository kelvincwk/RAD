using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
///TODO: Configure NuGet to download the latest EnterpriseLibrary packages
using ETL = Microsoft.Practices.EnterpriseLibrary;

namespace Switchboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Notification()
        {
            return View();
        }

        #region "OAuth, Web Hook Callbacks and OAuth Callbacks"
        /// <summary>
        /// Subscribe to cloud storage changes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public ActionResult SubscribeStorageChanges(string name, string email, string code)
        {
            //TODO: Configure the unity container to be a factory to pickup the mapTo concrete class for the implementation
            // This way we can have the site to swap from Google to Dropbox api or both in conditions
            Switchboard.Connectors.IStorageConnector drive = ETL.PolicyInjection.PolicyInjection.Create<Connectors.GoogleDriveClient, Connectors.IStorageConnector>();

            //If this is a callback?
            if (code != null) {
                /// TODO: encapsulate all session state access into the Session Information\User.cs
                HttpContext.Session.Add("loginWith", code);
            }

            // Assuming user has already configured his cloud drive to be synced against the cloud provider
            //TODO:  In the Watch() implementation it can look up in the user session by user name and email address
            //TODO:  Need to maintain list of user names and sessions at the scope of HttpApplicationState 
            string sessionId = "443hdsifi9223";
            drive.Watch(name, sessionId);

            return RedirectToAction("Notification");
        }

        /// <summary>
        /// Callback method from the cloud storage in the event of new watch changes
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("NotifyStorageChanges")]
        public ActionResult NotifyStorageChanges(string changes)
        {
            var resourceURI = this.HttpContext.Request.Headers["X-Goog-Resource-URI"];
            string resourceId;
            string resourceState;
            if (resourceURI != null)
            {
                resourceId = this.HttpContext.Request.Headers["X-Goog-Resource-ID"];
                resourceState = this.HttpContext.Request.Headers["X-Goog-Resource-State"];
                Switchboard.Notification.Instance.Notify(resourceId + "-" + resourceState + "-" + resourceURI);

                // Time to get list of changes
                Switchboard.Connectors.IStorageConnector storage = ETL.PolicyInjection.PolicyInjection.Create<Connectors.GoogleDriveClient, Connectors.IStorageConnector>();
                storage.GetChanges(resourceId);

                //Remember we are in the callback. Just redirect to ConnectDocumentCenter callback to make a copy of the changed documents into our designated documents center
                return ConnectDocumentCenter(resourceId, null);
            }
            else
            {
                //log the headers and details for investigations
                ETL.Logging.Logger.Write(new ETL.Logging.LogEntry { Message = "Unknown request - " + changes });
            }
            return Content(changes);
        }

        /// <summary>
        /// Callback method from the Exact open auth
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("ConnectDocumentCenter")]
        public ActionResult ConnectDocumentCenter(string resourceId, string token)
        {
            //TODO: if token is returned look up the resourceId against the token
            //If this is the straightforward invocation let's use the resourceId
            if (resourceId != null)
            {
                Switchboard.Notification.Instance.Notify("Downloading changes to your document center");
                //Instantiate the document client via the connector
                //TODO: Configure the unity container to be a factory to pickup the mapTo concrete class for the implementation so it can be Exact Online or Dropbox to make the copy of documents into
                //This demo is to invoke the Exact document client and insert the newly added file there
                Switchboard.Connectors.IStorageConnector documentClient = ETL.PolicyInjection.PolicyInjection.Create<Connectors.ExactDocumentClient, Connectors.IStorageConnector>();

                /// TODO: encapsulate all session state access into the Session Information\User.cs
                documentClient.Add(resourceId);
            }
            return Content(resourceId);
        }
        #endregion
    }
}