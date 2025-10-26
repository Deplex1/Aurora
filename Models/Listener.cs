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
        public string username { get; set; }
        public string? password { get; set; }

        // store raw image bytes (from DB)
        public byte[]? ProfilePictureBytes { get; set; }

        // auto-convert for displaying in <img src="">
        public string? ProfilePictureBase64
        {
            get
            {
                if (ProfilePictureBytes != null)
                    return $"data:image/png;base64,{Convert.ToBase64String(ProfilePictureBytes)}";
                return null;
            }
        }

        public Listener(int id, string username)
        {
            this.userID = id;
            this.username = username;
        }

        public Listener() { }
    }

}
