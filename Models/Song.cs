public class Song
{
    public int SongId { get; set; }
    public string Title { get; set; }
    public int Duration { get; set; }
    public string FilePath { get; set; }
    public int UserId { get; set; }
    public int GenreId { get; set; }

    public Song() { }
    public Song(int songId, string title, int duration, string filePath, int userId, int genreId)
    {
        SongId = songId;
        Title = title;
        Duration = duration;
        FilePath = filePath;
        UserId = userId;
        GenreId = genreId;
    }
}
