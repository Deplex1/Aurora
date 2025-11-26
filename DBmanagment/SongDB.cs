using Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace DBL
{
    public class SongDB : BaseDB<Song>
    {
        // Return table name for all BaseDB SQL generators.
        protected override string GetTableName()
        {
            return "songs";
        }

        // Define the primary key column so BaseDB can find inserted rows.
        protected override string GetPrimaryKeyName()
        {
            return "songid";
        }

        // Convert a single SQL row array to a Song model instance.
        protected async override Task<Song> CreateModelAsync(object[] row)
        {
            // songid, title, artist, genreid, filepath, duration
            return new Song(
                int.Parse(row[0].ToString()),
                row[1].ToString(),
                row[2].ToString(),
                int.Parse(row[3].ToString()),
                row[4].ToString(),
                int.Parse(row[5].ToString())
            );
        }

        // Query multiple rows using a title LIKE filter.
        public async Task<List<Song>> SelectLikeAsync(string titleSearch)
        {
            // Build the SQL manually since the BaseDB runner is private.
            string table = GetTableName();
            string sql = $"SELECT * FROM {table} WHERE title LIKE @search";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "search", $"%{titleSearch}%" }
            };

            List<object[]> rows = await ExecuteReaderRowsAsync(sql, parameters);
            return await CreateListModelAsync(rows);
        }

        // Query a single row by primary key.
        public async Task<Song?> SelectSingleAsync(int id)
        {
            string table = GetTableName();
            string pk = GetPrimaryKeyName();
            string sql = $"SELECT * FROM {table} WHERE {pk} = @id LIMIT 1";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "id", id }
            };

            List<object[]> rows = await ExecuteReaderRowsAsync(sql, parameters);

            if (rows.Count == 1)
            {
                return await CreateModelAsync(rows[0]);
            }

            return null;
        }

        // Insert a song record into the DB and return the inserted object.
        public async Task<Song> InsertSongAsync(Song s)
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                { "title", s.title },
                { "artist", s.artist },
                { "duration", s.duration },
                { "filepath", s.filePath }
            };

            return await InsertGetObjAsync(values);
        }

        // Delete a song by ID and return affected row count.
        public async Task<int> DeleteSongAsync(int id)
        {
            string table = GetTableName();
            string pk = GetPrimaryKeyName();
            string sql = $"DELETE FROM {table} WHERE {pk} = @id";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "id", id }
            };

            return await ExecuteNonQueryAsync(sql, parameters);
        }

        // ------------------------------------------------------------------
        // TEMP FIX ➤ internal row executor since your BaseDB runner is private.
        // This manually rebuilds minimal SQL execution logic using your DB infra.
        // ------------------------------------------------------------------

        private async Task<List<object[]>> ExecuteReaderRowsAsync(string sql, Dictionary<string, object> parameters)
        {
            List<object[]> list = new List<object[]>();

            using (var c = conn.CreateCommand())
            {
                c.CommandText = sql;

                foreach (var p in parameters)
                {
                    var param = c.CreateParameter();
                    param.ParameterName = "@" + p.Key;
                    param.Value = p.Value ?? DBNull.Value;
                    c.Parameters.Add(param);
                }

                await conn.OpenAsync();
                using (var r = await c.ExecuteReaderAsync())
                {
                    int count = r.FieldCount;
                    while (await r.ReadAsync())
                    {
                        object[] row = new object[count];
                        r.GetValues(row);
                        list.Add(row);
                    }
                }
                await conn.CloseAsync();
            }

            return list;
        }

        private async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters)
        {
            int affected = 0;

            using (var c = conn.CreateCommand())
            {
                c.CommandText = sql;

                foreach (var p in parameters)
                {
                    var param = c.CreateParameter();
                    param.ParameterName = "@" + p.Key;
                    param.Value = p.Value ?? DBNull.Value;
                    c.Parameters.Add(param);
                }

                await conn.OpenAsync();
                affected = await c.ExecuteNonQueryAsync();
                await conn.CloseAsync();
            }

            return affected;
        }
    }
}
