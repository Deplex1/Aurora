using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class SongDB : BaseDB<Song>
    {
        protected override string GetTableName()
        {
            return "songs";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        protected async override Task<Song> CreateModelAsync(object[] row)
        {
            return new Song(
                int.Parse(row[0].ToString()),
                row[1].ToString(),
                row[2].ToString(),
                int.Parse(row[3].ToString()),
                row[4].ToString()
            );
        }

        public async Task<Song> InsertSongAsync(Song s)
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                { "title", s.title },
                { "artist", s.artist },
                { "genreid", s.genreID },
                { "filepath", s.filePath }
            };

            return (Song)await base.InsertGetObjAsync(values);
        }

        public async Task<List<Song>> GetAllAsync()
        {
            return (List<Song>)await SelectAllAsync();
        }

        public async Task<Song?> GetByIdAsync(int id)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>()
            {
                { "songid", id }
            };

            return await SelectSingleAsync(filter);
        }

        public async Task<List<Song>> SearchByTitleAsync(string title)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>()
            {
                { "title", $"%{title}%" }
            };

            return await SelectLikeAsync(filter);
        }

        public async Task<int> DeleteSongAsync(int id)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>()
            {
                { "songid", id }
            };

            return await DeleteAsync(filter);
        }
    }
}
