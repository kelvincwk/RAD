using System;
using System.Web;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using ETL = Microsoft.Practices.EnterpriseLibrary;

namespace Switchboard
{
    /// <summary>
    /// This represents a signalR hub which returns a singleton notification class to get hold of all clients
    /// Clients then can be referenced and validated individually
    /// </summary>
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        private readonly Notification _notification;

        public NotificationHub() : this(Notification.Instance) { }

        public NotificationHub(Notification notification)
        {
            _notification = notification;
        }

        public void Notify(string name, string message)
        {
            _notification.Notify(name, message);
        }
        public void NotifyStorageChanges(List<Models.Item> items)
        {
            _notification.NotifyStorageChanges(items);
        }
    }

    public class Notification
    {
        const string __SYSTEM_ID = "System";
     
        // Invoke singleton constructor
        private readonly static Lazy<Notification> _instance = new Lazy<Notification>(() => new Notification(GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients));
        private IHubConnectionContext Clients;

        /// <summary>
        /// Gets hold of clients
        /// </summary>
        /// <param name="clients"></param>
        public Notification(IHubConnectionContext clients)
        {
            this.Clients = clients;
        }

        /// <summary>
        /// Returns the singleton
        /// </summary>
        public static Notification Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Purely send messages for debugging
        /// </summary>
        /// <param name="message"></param>
        public void Notify(string message)
        {
            // Call the addNewMessageToPage method to update clients.
            //TODO: This should be implemented to only notify the client in current client if name != "System"
            Notify(__SYSTEM_ID, message);
        }
        /// <summary>
        /// Purely send messages for debugging
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        public void Notify(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            //TODO: This should be implemented to only notify the client in current client if name != "System"
            Clients.All.addNewMessageToPage(name, message);
        }
        /// <summary>
        /// Notify client with storage chnages
        /// </summary>
        /// <param name="name"></param>
        /// <param name="items"></param>
        public void NotifyStorageChanges(List<Models.Item> items)
        {
            //TODO: This should be implemented to only notify the client in current client if name != "System"
            Clients.All.displayDriveChangesNotification(items.Count);
        }
    }
}