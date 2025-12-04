using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class SongsDB : BaseDB<Song>
    {
        protected override string GetTableName() => "songs";
        protected override string GetPrimaryKeyName() => "songid";

        protected override async Task<Song> CreateModelAsync(object[] row)
        {
            return new Song(
                int.Parse(row[0].ToString()),                 // songid
                row[1].ToString(),                            // title
                row[2].ToString(),                            // artist
                int.Parse(row[3].ToString()),                 // genreid
                int.Parse(row[4].ToString()),                 // duration
                row[5].ToString()                             // filePath
            );
        }

        // 🔹 Get all songs
        public async Task<List<Song>> GetAllAsync()
        {
            return await SelectAllAsync();
        }

        // 🔹 Get song by primary key
        public async Task<Song?> GetSongByPkAsync(int songId)
        {
            var p = new Dictionary<string, object> { { "songid", songId } };
            var list = await SelectAllAsync(p);
            return list.Count == 1 ? list[0] : null;
        }

        // 🔹 Insert a new song
        public async Task<Song> InsertGetObjAsync(Song song)
        {
            var values = new Dictionary<string, object>
            {
                { "title", song.Title },
                { "artist", song.Artist },
                { "genreid", song.GenreId },
                { "duration", song.Duration },
                { "filepath", song.FilePath }
            };

            return await base.InsertGetObjAsync(values);
        }

        // 🔹 Update a song
        public async Task<int> UpdateAsync(Song song)
        {
            var values = new Dictionary<string, object>
            {
                { "title", song.Title },
                { "artist", song.Artist },
                { "genreid", song.GenreId },
                { "duration", song.Duration },
                { "filepath", song.FilePath }
            };
            var filter = new Dictionary<string, object> { { "songid", song.SongId } };

            return await base.UpdateAsync(values, filter);
        }

        // 🔹 Delete a song
        public async Task<int> DeleteAsync(Song song)
        {
            var filter = new Dictionary<string, object> { { "songid", song.SongId } };
            return await base.DeleteAsync(filter);
        }

        // 🔹 Get songs by genre
        public async Task<List<Song>> GetSongsByGenreAsync(int genreId)
        {
            var p = new Dictionary<string, object> { { "genreid", genreId } };
            return await SelectAllAsync(p);
        }

        // 🔹 Search songs by title or artist
        public async Task<List<Song>> SearchAsync(string query)
        {
            var sql = $"SELECT * FROM {GetTableName()} WHERE title LIKE @q OR artist LIKE @q";
            var parameters = new Dictionary<string, object> { { "q", $"%{query}%" } };
            return await SelectAllAsync(sql, parameters);
        }
    }
}
