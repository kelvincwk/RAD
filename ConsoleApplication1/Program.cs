using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Switchboard;
using Switchboard.Connectors;
using ExactOnline.Client.Sdk.Controllers;

namespace ConsoleApplication1
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string clientId;
                string clientSecret;
                string storageAuthenticatedCallbackRedirectURL;
                ExactConnectorHelper connector;
                ExactOnlineClient exactClient;

                clientId = "4cb5d830-4192-4cbc-b245-dff0c8ad18e2";
                clientSecret = "mShp7gyt3Fz7";
                storageAuthenticatedCallbackRedirectURL = "https://googledrivewatcher.azurewebsites.net/Home/ConnectDocumentCenter";
                connector = new ExactConnectorHelper(clientId, clientSecret, new Uri(storageAuthenticatedCallbackRedirectURL));
                exactClient = new ExactOnlineClient(connector.EndPoint, connector.GetAccessToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
