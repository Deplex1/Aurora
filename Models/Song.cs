public class Song
{
    public int SongId { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }   // new
    public int GenreId { get; set; }
    public int Duration { get; set; }
    public string FilePath { get; set; }

    public Song() { }
    public Song(int songId, string title, string artist, int genreId, int duration, string filePath)
    {
        SongId = songId;
        Title = title;
        Artist = artist;
        GenreId = genreId;
        Duration = duration;
        FilePath = filePath;
    }
}
