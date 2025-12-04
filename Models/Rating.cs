public class Rating
{
    public int RatingId { get; set; }
    public int UserId { get; set; }
    public int SongId { get; set; }
    public int Rate { get; set; }
    public DateTime? DateRated { get; set; }

    public Rating() { }

    public Rating(int ratingId, int userId, int songId, int rate, DateTime? dateRated)
    {
        RatingId = ratingId;
        UserId = userId;
        SongId = songId;
        Rate = rate;
        DateRated = dateRated;
    }
}
