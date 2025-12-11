using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class SongsDB : BaseDB<Song>
    {
        protected override string GetTableName()
        {
            return "songs";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        protected override async Task<Song> CreateModelAsync(object[] row)
        {
            return new Song(
                int.Parse(row[0].ToString()), // songid
                row[1].ToString(),            // title
                int.Parse(row[2].ToString()), // duration
                row[3].ToString(),            // filepath
                int.Parse(row[4].ToString()), // userid
                int.Parse(row[5].ToString())  // genreid
            );
        }

        public async Task<List<Song>> SelectAllAsync()
        {
            return await base.SelectAllAsync();
        }

        public async Task<Song> InsertSongAsync(string title, int duration, string filePath, int userId, int genreId)
        {
            var values = new Dictionary<string, object>
            {
                { "title", title },
                { "duration", duration },
                { "filepath", filePath },
                { "userid", userId },
                { "genreid", genreId }
            };
            return await base.InsertGetObjAsync(values);
        }

        public async Task<int> SaveDurationAsync(int songId, int duration)
        {
            var values = new Dictionary<string, object>
            {
                { "duration", duration }
            };
            var parameters = new Dictionary<string, object>
            {
                { "songid", songId }
            };
            return await base.UpdateAsync(values, parameters);
        }

        public async Task<List<Song>> SearchLikeAsync(string term)
        {
            string sql = "SELECT * FROM songs WHERE title LIKE @term";
            var parameters = new Dictionary<string, object>
            {
                { "term", $"%{term}%" }
            };
            return await SelectAllAsync(sql, parameters);
        }

        public async Task<Song> SelectSingleAsync(int id)
        {
            var parameters = new Dictionary<string, object>
            {
                { "songid", id }
            };
            var list = await SelectAllAsync(parameters);
            return list.Count > 0 ? list[0] : null;
        }
    }
}
