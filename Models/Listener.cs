using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Listener
    {
        public int userID { get; set; }
        
        public string email { get; set; }
        public string username { get; set; }
        public string? pfpLink { get; set; }
        public bool IsAdmin { get; set; }

        public Listener(int userID, string username, string email, string pfpLink, bool IsAdmin) 
        {
            this.userID = userID;
            this.email = email;
            this.username = username;
            this.pfpLink = pfpLink;
            this.IsAdmin = IsAdmin;
        }

        public Listener(int userID, string username, string email, bool IsAdmin)
        {
            this.userID = userID;
            this.email = email;
            this.username = username;
            this.IsAdmin = IsAdmin;
        }

        public Listener(string username, string email)
        {
            this.username = username;
            this.email = email;
        }




    }

}
