namespace Models
{
    public class Genre
    {
        public int genreID { get; set; }
        public string name { get; set; }

        public Genre()
        {
            this.genreID = 0;
            this.name = string.Empty;
        }

        public Genre(int genreID, string name)
        {
            this.genreID = genreID;
            this.name = name;
        }
    }
}
