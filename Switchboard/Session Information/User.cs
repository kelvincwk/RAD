using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Switchboard.Session
{
    /// <summary>
    /// Provides the abstract to access the session states
    /// </summary>
    public static class User
    {
        static User()
        {

        }
        public static Switchboard.Entities.User Current
        {
            get
            {
                //TODO: implement to retrieve from session
                return new Switchboard.Entities.User() { ResourceId = "1 - pmMwwPf3eg07S_2W6MBKB1qI8RzUi9V" };
            }
            set
            {
                //TODO: implement to store into session
            }
        }
    }
}