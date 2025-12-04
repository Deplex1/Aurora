using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class RatingsDB : BaseDB<Rating>
    {
        protected override string GetTableName() => "ratings";
        protected override string GetPrimaryKeyName() => "ratingid";

        protected override async Task<Rating> CreateModelAsync(object[] row)
        {
            return new Rating(
                int.Parse(row[0].ToString()),   // ratingid
                int.Parse(row[1].ToString()),   // userid
                int.Parse(row[2].ToString()),   // songid
                int.Parse(row[3].ToString()),   // rating
                row[4] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row[4]) : null // daterated
            );
        }

        // 🔹 Get all ratings
        public async Task<List<Rating>> GetAllAsync()
        {
            return await SelectAllAsync();
        }

        // 🔹 Get rating by primary key
        public async Task<Rating?> GetRatingByPkAsync(int ratingId)
        {
            var p = new Dictionary<string, object> { { "ratingid", ratingId } };
            var list = await SelectAllAsync(p);
            return list.Count == 1 ? list[0] : null;
        }

        // 🔹 Add a new rating
        public async Task<Rating> InsertGetObjAsync(Rating rating)
        {
            if (rating.Rate < 1 || rating.Rate > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            var values = new Dictionary<string, object>
            {
                { "userid", rating.UserId },
                { "songid", rating.SongId },
                { "rating", rating.Rate }
            };

            return await base.InsertGetObjAsync(values);
        }

        // 🔹 Update a rating
        public async Task<int> UpdateAsync(Rating rating)
        {
            if (rating.Rate < 1 || rating.Rate > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            var values = new Dictionary<string, object> { { "rating", rating.Rate } };
            var filter = new Dictionary<string, object> { { "ratingid", rating.RatingId } };

            return await base.UpdateAsync(values, filter);
        }

        // 🔹 Delete a rating
        public async Task<int> DeleteAsync(Rating rating)
        {
            var filter = new Dictionary<string, object> { { "ratingid", rating.RatingId } };
            return await base.DeleteAsync(filter);
        }

        // 🔹 Get all ratings for a song
        public async Task<List<Rating>> GetRatingsBySongAsync(int songId)
        {
            var p = new Dictionary<string, object> { { "songid", songId } };
            return await SelectAllAsync(p);
        }

        // 🔹 Get all ratings by a user
        public async Task<List<Rating>> GetRatingsByUserAsync(int userId)
        {
            var p = new Dictionary<string, object> { { "userid", userId } };
            return await SelectAllAsync(p);
        }

        // 🔹 Get rating for a specific user and song
        public async Task<Rating?> GetRatingByUserAndSongAsync(int userId, int songId)
        {
            var p = new Dictionary<string, object>
            {
                { "userid", userId },
                { "songid", songId }
            };
            var list = await SelectAllAsync(p);
            return list.Count == 1 ? list[0] : null;
        }
    }
}
