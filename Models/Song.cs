namespace Models
{
    public class Song
    {
        public int songID { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public int genreID { get; set; }
        public string filePath { get; set; }

        public Song(int songID, string title, string artist, int genreID, string filePath)
        {
            this.songID = songID;
            this.title = title;
            this.artist = artist;
            this.genreID = genreID;
            this.filePath = filePath;
        }
    }
}
