using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class GenereDB : BaseDB<Genre>
    {
        protected override string GetTableName()
        {
            return "genres";
        }

        protected override string GetPrimaryKeyName()
        {
            return "genreid";
        }

        protected async override Task<Genre> CreateModelAsync(object[] row)
        {
            return new Genre(
                int.Parse(row[0].ToString()),
                row[1].ToString()
            );
        }

        public async Task<Genre> InsertGenre(Genre g)
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                { "name", g.name }
            };

            return (Genre)await base.InsertGetObjAsync(values);
        }

        public async Task<List<Genre>> GetAllAsync()
        {
            return (List<Genre>)await SelectAllAsync();
        }

        public async Task<int> DeleteGenre(int id)
        {
            Dictionary<string, object> filter = new Dictionary<string, object>()
            {
                { "genreid", id }
            };

            return await base.DeleteAsync(filter);
        }
    }
}
