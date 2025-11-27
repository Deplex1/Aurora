using System;

namespace Models
{
    public class Song
    {
        public int songID { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public int genreID { get; set; }
        public int duration { get; set; } // duration in seconds
        public string filePath { get; set; }

        // Constructor
        public Song(int songID, string title, string artist, int genreID, int duration, string filePath)
        {
            this.songID = songID;
            this.title = title;
            this.artist = artist;
            this.genreID = genreID;
            this.duration = duration;
            this.filePath = filePath;
        }

        // Empty constructor for ORM/blazor binding if needed
        public Song() { }
    }
}
