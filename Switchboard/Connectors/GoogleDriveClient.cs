using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Auth.OAuth2;
//using Google.Apis.Sheets.v4;
//using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Drive;
using Google.Apis.Drive.v2;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using Google.Apis.Drive.v2.Data;
using Switchboard.Models;
using ETL = Microsoft.Practices.EnterpriseLibrary;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using System.Net;

namespace Switchboard.Connectors
{
    /// <summary>
    /// Class that works with Google Drive cloud api
    /// TODO: Methods are supposed to be coded in async
    /// </summary>
    public class GoogleDriveClient : Connectors.StorageConnectorBase, IStorageConnector
    {
        #region "Private Members"
        private string[] _scopes;
        private string _applicationName;
        private string _storageChangedNotificationRedirectURL;
        private UserCredential _credential;
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public GoogleDriveClient()
        {
            _scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveAppdata, DriveService.Scope.DriveFile };
            ///TODO: These all can be set in the web.config file and be transformed in the publishing scripts if required
            _applicationName = "Google Drive API .NET Quickstart";
            //_storageChangedNotificationRedirectURL = @"https://googledrivewatcher.azurewebsites.net/Home/NotifyStorageChanges";
            _storageChangedNotificationRedirectURL = @"http://localhost:55410/Home/NotifyStorageChanges";
        }

        /// <summary>
        /// Keeps the secrets in this private struc
        /// </summary>
        private static ClientSecrets secrets = new ClientSecrets()
        {
            ///TODO: These all can be set in the web.config file and be transformed in the publishing scripts if required
            ClientId = "366262687923-e2rt4nqp99nh1cv2e6ha2l7duq0bab0p.apps.googleusercontent.com",
            ClientSecret = "60plblaE8VE3bPSszdzz899j"
        };

        // Google access token
        private class GoogleAccessToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string id_token { get; set; }
            public string refresh_token { get; set; }
        }

        /// <summary>
        /// Authorise the app from the cloud API via the implemented connector
        /// </summary>
        protected override bool Authorise()
        {
            try
            {
                bool runLocally = (System.Configuration.ConfigurationManager.AppSettings["RunLocally"].ToString() == "true");
                ///Toggle between the debug mode and potentially the firewall access issues
                if (!runLocally)
                {
                    ///Running over the azure
                    string authRedirectURL = String.Empty;
                    //authRedirectURL = "https://googledrivewatcher.azurewebsites.net/Home/SubscribeStorageChanges";
                    authRedirectURL = "http://localhost:55410/Home/SubscribeStorageChanges";
                    if (!GoogleAuthInitialized())
                    {
                        //var Googleurl = "https://accounts.google.com/o/oauth2/auth?response_type=code&access_type=offline&approval_prompt=force&redirect_uri=" + authRedirectURL + "&scope=https://www.googleapis.com/auth/userinfo.email%20https://www.googleapis.com/auth/userinfo.profile&client_id=" + secrets.ClientId;
                        var Googleurl = "https://accounts.google.com/o/oauth2/auth?response_type=code&access_type=offline&approval_prompt=force&redirect_uri=" + authRedirectURL + "&scope=https://www.googleapis.com/auth/drive&client_id=" + secrets.ClientId;
                        /// TODO: encapsulate all session state access into the Session Information\User.cs
                        HttpContext.Current.Session["loginWith"] = "google";
                        HttpContext.Current.Response.Redirect(Googleurl);
                        return false;
                    }

                    var url = HttpContext.Current.Request.Url.Query;
                    if (url != "")
                    {
                        var code = ExtractAccount(url);

                        if (code != null)
                        {
                            //get the access token 
                            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
                            webRequest.Method = "POST";
                            string Parameters = "grant_type=authorization_code&code=" + code + "&client_id=" + secrets.ClientId + "&client_secret=" + secrets.ClientSecret + "&redirect_uri=" + authRedirectURL;
                            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(Parameters);
                            webRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                            webRequest.ContentLength = byteArray.Length;
                            Stream postStream = webRequest.GetRequestStream();
                            // Add the post data to the web request
                            postStream.Write(byteArray, 0, byteArray.Length);
                            postStream.Close();

                            WebResponse response = webRequest.GetResponse();
                            postStream = response.GetResponseStream();
                            StreamReader reader = new StreamReader(postStream);
                            string responseFromServer = reader.ReadToEnd();

                            GoogleAccessToken serStatus = JsonConvert.DeserializeObject<GoogleAccessToken>(responseFromServer);

                            if (serStatus != null)
                            {
                                serStatus.id_token = "kelvincheah06@gmail.com";
                                string accessToken = string.Empty;
                                accessToken = serStatus.access_token;

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    // This is where you want to add the code if login is successful.
                                    // getgoogleplususerdataSer(accessToken);

                                    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                                    {
                                        ClientSecrets = new ClientSecrets
                                        {
                                            ClientId = secrets.ClientId,
                                            ClientSecret = secrets.ClientSecret
                                        },
                                        Scopes = _scopes,
                                        DataStore = new AppDataFileStore(".credentials")
                                    });

                                    var token = new TokenResponse
                                    {
                                        AccessToken = accessToken,
                                        RefreshToken = serStatus.refresh_token
                                    };

                                    _credential = new UserCredential(flow, serStatus.id_token, token);
                                    return (_credential != null);
                                }
                            }

                        }
                    }
                }
                else
                {
                    //Below code does not work in Azure it is not allowed to mimic the browser
                    using (var stream =
                        new FileStream(HttpContext.Current.Request.MapPath("~/client_secret.json"), FileMode.Open, FileAccess.Read))
                    {
                        //string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                        string credPath = HttpContext.Current.Request.MapPath("~/App_Data/.credentials/drive.googleapis.com-dotnet-quickstart.json");
                        //credPath = Path.Combine(credPath, ".credentials\\drive.googleapis.com-dotnet-quickstart.json");

                        _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            _scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                        Console.WriteLine("Credential file saved to: " + credPath);
                    }
                    return (_credential != null);

                    //Use the code exchange flow to get an access and refresh token.
                        //IAuthorizationCodeFlow flow =
                        //    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                        //    {
                        //        ClientSecrets = secrets,
                        //        Scopes = _scopes
                        //    });
                }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message, ex);
                ETL.Logging.Logger.Write(new ETL.Logging.LogEntry { Message = ex.Message });
                return false;
            }
            return false;
        }

        private string ExtractAccount(string url)
        {
            string queryString = url.ToString();
            char[] delimiterChars = { '=' };
            string[] words = queryString.Split(delimiterChars);
            if (words.Length >= 1)
            {
                string code = words[1];
                return code;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool GoogleAuthInitialized()
        {
            /// TODO: encapsulate all session state access into the Session Information\User.cs
            return ((HttpContext.Current.Session.Contents.Count > 0) && (HttpContext.Current.Session["loginWith"] != null));
        }

        /// <summary>
        /// Subscribe to google drive watch request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sessionId"></param>
        public void Watch(string name, string sessionId)
        {
            ///If unauthorised it will be going to the OAuth2 process then revisited again in the callback (see HomeController.cs)
            if (!Authorise()) { return; }

            var userSettings = Session.User.Current;

            ///Create a channel resource to be recognized by google
            Google.Apis.Drive.v2.Data.Channel channel = new Channel()
            {
                ResourceId = userSettings.ResourceId,
                Id = Guid.NewGuid().ToString(),
                Address = string.Concat(_storageChangedNotificationRedirectURL, "?user=", name),
                Type = "web_hook"
                //Expiration = 2*60*1000 //2 minutes??? Nope not quite work out!
            };

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });
            try
            {
                //var stopRequest = service.Channels.Stop(new Channel { Id = Guid.NewGuid().ToString(), ResourceId = "4k6re2lcW7mV0ys2HnyM5E0eVY" });
                //stopRequest.Execute();

                ChangesResource resource = new ChangesResource(service);

                var startPageRequest = resource.GetStartPageToken();
                var startPageToken = startPageRequest.Execute();

                //Google Drive APIs > REST v2 > Changes.getStartPageToken()
                // store the startPageToken to the user session so it can be reused to retrieve list of changes
                userSettings.LastSyncedPageToken = startPageToken.StartPageTokenValue;

                //Google Drive APIs > REST v2 > Changes.watch()
                var watchRequest = resource.Watch(channel);
                watchRequest.PageToken = startPageToken.StartPageTokenValue;

                //FilesResource fileResource = new FilesResource(service);               
                //var watchRequest = fileResource.Watch(channel, "1-pmMwwPf3eg07S_2W6MBKB1qI8RzUi9V");

                watchRequest.Execute();
                //return watchRequest;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.StackTrace);
                //log error
                ETL.Logging.Logger.Write(new ETL.Logging.LogEntry { Message = exc.Message });
            }
        }

        /// <summary>
        /// Gets list of changes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public List<Item> GetChanges(string name, string resourceId)
        {
            //TODO: Disable this for demo only
            //if (!Authorise()) { return null; }

            // Look up the user session against the resourceId
            var lastPageToken = Switchboard.Session.User.Current.LastSyncedPageToken;

            //TODO: implement to get list of changes from the cloud
            //TODO: Google Drive APIs > REST v2 > Changes.list(pageToken)
            //TODO: Google Drive APIs > REST v2 > Files.get()
            //For testing hereby hardcoded a file ID as the resource Id
            List<Item> changes = new List<Item>();
            changes.Add(new Item { FileName = "Google random file", MIMEType = "plain/text",
                ResourceId = "1ypUeUNLr0PzJ4spbu6UnvVmPt0JUeZTgeF1Wo6Ax1_c", DownloadURL = "https://www.googleapis.com/drive/v3/files/1ypUeUNLr0PzJ4spbu6UnvVmPt0JUeZTgeF1Wo6Ax1_c" });

            Switchboard.Notification.Instance.NotifyStorageChanges(changes);
            changes.ForEach(delegate (Item item)
            {
                Switchboard.Notification.Instance.Notify(name, string.Concat(string.Format("You have {0} new changes on your drive in the cloud", changes.Count),
                    "New change to resource ", item.ResourceId, " please download here ", System.Web.HttpUtility.JavaScriptStringEncode(item.DownloadURL)));
            });

            ///Chuck the files to be synced into the session
            ///For demo purposes there will be only one hardcoded file to be added
            /// TODO: encapsulate all session state access into the Session Information\User.cs
            HttpContext.Current.Session["syncItem"] = changes[0];
            return changes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resourceId"></param>
        public void Add(string name, string resourceId)
        {
            throw new NotImplementedException();
        }
    }
}