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
            this.password = string.Empty;
            this.email = string.Empty;
            this.profilepicture = null;
            this.userid = 0;
            this.IsAdmin = 0;
            this.ResetCode = string.Empty;
        }

        public int userid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public byte[]? profilepicture { get; set; }
        public int IsAdmin { get; set; }
        public string ResetCode { get; set; }

        public Listener(int userid, string username, string password, string email, byte[]? profilepicture, int IsAdmin, string ResetCode)
        {
            this.userid = userid;
            this.username = username;
            this.password = password;
            this.email = email;
            this.profilepicture = profilepicture;
            this.IsAdmin = IsAdmin;
            this.ResetCode = ResetCode;
        }

        public Listener(int userid, string username, string email, byte[]? profilepicture, int IsAdmin)
        {
            this.userid = userid;
            this.username = username;
            this.email = email;
            this.profilepicture = profilepicture;
            this.IsAdmin = IsAdmin;
        }

        public Listener(string username, string email)
        {
            this.username = username;
            this.email = email;
        }

        public Listener(Listener ls)
        {
            this.userid = ls.userid;
            this.username = ls.username;
            this.password = ls.password;
            this.email = ls.email;
            this.profilepicture = ls.profilepicture;
            this.IsAdmin = ls.IsAdmin;
            this.ResetCode = ls.ResetCode;
        }
    }
}
