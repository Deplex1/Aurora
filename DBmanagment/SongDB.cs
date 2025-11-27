using Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DBL
{
    public class SongDB : BaseDB<Song>
    {
        // Return target table name
        protected override string GetTableName()
        {
            return "songs";
        }

        // Return primary key column name
        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        // Create a Song model from a row result
        protected async override Task<Song> CreateModelAsync(object[] row)
        {
            return new Song(
                int.Parse(row[0].ToString()),
                row[1].ToString(),
                row[2].ToString(),
                int.Parse(row[3].ToString()),
                int.Parse(row[4].ToString()),
                row[5].ToString()
            );
        }

        // Insert a song and return inserted object
        public async Task<Song?> InsertSongAsync(Song s)
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                { "title", s.title },
                { "artist", s.artist },
                { "genreid", s.genreID },
                { "duration", s.duration },
                { "filepath", s.filePath }
            };

            Song result = await base.InsertGetObjAsync(values);
            return result != null ? result : null;
        }

        // Get all songs
        public async Task<List<Song>> GetAllAsync()
        {
            return await SelectAllAsync();
        }

        // Search songs by title or artist using LIKE
        public async Task<List<Song>> SearchAsync(string query)
        {
            var all = await SelectAllAsync();

            if (string.IsNullOrWhiteSpace(query))
                return all;

            string q = query.ToLower();

            return all.Where(s =>
                s.title.ToLower().Contains(q) ||
                s.artist.ToLower().Contains(q)
            ).ToList();
        }

        // Delete a song by id
        public async Task<int> DeleteSongAsync(int id)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>()
            {
                { "songid", id }
            };

            return await DeleteAsync(filter);
        }

        // OPTIONAL: Get a single song by ID (manual filtering)
        public async Task<Song?> GetByIdAsync(int id)
        {
            var all = await SelectAllAsync();
            return all.FirstOrDefault(s => s.songID == id);
        }
    }
}
