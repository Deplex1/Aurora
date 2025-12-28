using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class SongArtistsDB : BaseDB<SongArtist>
    {
        protected override string GetTableName()
        {
            return "song_artists";
        }

        protected override string GetPrimaryKeyName()
        {
            return "songid,userid"; // Composite primary key
        }

        protected override async Task<SongArtist> CreateModelAsync(object[] row)
        {
            return new SongArtist(
                int.Parse(row[0].ToString()), // songid
                int.Parse(row[1].ToString()), // userid
                row[2].ToString(),            // role
                DateTime.Parse(row[3].ToString()) // added_date
            );
        }

        public async Task<List<SongArtist>> SelectAllAsync()
        {
            return await base.SelectAllAsync();
        }

        public async Task AddSongArtistAsync(int songId, int userId, string role = "main")
        {
            string sql = "INSERT INTO song_artists (songid, userid, role) VALUES (@songid, @userid, @role)";
            var parameters = new Dictionary<string, object>
            {
                { "songid", songId },
                { "userid", userId },
                { "role", role }
            };
            await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task RemoveSongArtistAsync(int songId, int userId)
        {
            string sql = "DELETE FROM song_artists WHERE songid = @songid AND userid = @userid";
            var parameters = new Dictionary<string, object>
            {
                { "songid", songId },
                { "userid", userId }
            };
            await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<List<int>> GetSongArtistsAsync(int songId)
        {
            string sql = "SELECT userid FROM song_artists WHERE songid = @songid ORDER BY role, userid";
            var parameters = new Dictionary<string, object>
            {
                { "songid", songId }
            };
            return await SelectScalarListAsync<int>(sql, parameters);
        }

        public async Task<List<int>> GetArtistSongsAsync(int artistId)
        {
            string sql = "SELECT songid FROM song_artists WHERE userid = @userid";
            var parameters = new Dictionary<string, object>
            {
                { "userid", artistId }
            };
            return await SelectScalarListAsync<int>(sql, parameters);
        }

        public async Task UpdateSongArtistsAsync(int songId, List<int> artistIds, string role = "main")
        {
            // First, remove all existing artists for this song
            string deleteSql = "DELETE FROM song_artists WHERE songid = @songid";
            var deleteParams = new Dictionary<string, object> { { "songid", songId } };
            await ExecuteNonQueryAsync(deleteSql, deleteParams);

            // Then add the new artists
            foreach (var artistId in artistIds)
            {
                await AddSongArtistAsync(songId, artistId, role);
            }
        }

        public async Task<List<SongArtist>> GetSongArtistsWithDetailsAsync(int songId)
        {
            string sql = "SELECT * FROM song_artists WHERE songid = @songid ORDER BY role, userid";
            var parameters = new Dictionary<string, object>
            {
                { "songid", songId }
            };
            return await SelectAllAsync(sql, parameters);
        }
    }
}
