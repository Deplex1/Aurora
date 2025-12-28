using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class SongArtist
    {
        public int SongId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
        public DateTime AddedDate { get; set; }

        public SongArtist() { }

        public SongArtist(int songId, int userId, string role, DateTime addedDate)
        {
            SongId = songId;
            UserId = userId;
            Role = role;
            AddedDate = addedDate;
        }
    }
}
