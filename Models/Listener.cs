using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Listener
    {
        public Listener() 
        {
            this.username = string.Empty;
            this.email = string.Empty;
            this.pfpLink = string.Empty;
            this.userID = 0;
            this.IsAdmin = 0;
            this.reset_code = string.Empty;
        }
        public int userID { get; set; }
        
        public string email { get; set; }
        public string username { get; set; }
        public string? pfpLink { get; set; }
        public int IsAdmin { get; set; }
        
        public string reset_code { get; set; }

        public Listener(int userID, string username, string email, string pfpLink, int IsAdmin) 
        {
            this.userID = userID;
            this.email = email;
            this.username = username;
            this.pfpLink = pfpLink;
            this.IsAdmin = IsAdmin;
        }
        public Listener(int userID, string username, string email, string pfpLink, int IsAdmin, string resendCode)
        {
            this.userID = userID;
            this.email = email;
            this.username = username;
            this.pfpLink = pfpLink;
            this.IsAdmin = IsAdmin;
            this.reset_code = resendCode;

        }

        public Listener(int userID, string username, string email, int IsAdmin)
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

        public Listener(Listener ls)
        { 
            this.pfpLink = ls.pfpLink;  
            this.userID = ls.userID;
            this.email = ls.email;
            this.username = ls.username;
            this.IsAdmin = ls.IsAdmin;
        }
        


    }

}
