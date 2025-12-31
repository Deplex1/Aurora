// =================================================================================
// RaitngsDB.cs - DATABASE ACCESS LAYER FOR RATINGS SYSTEM
// =================================================================================
// This file handles all database operations for the ratings table.
// Table structure: ratingid, userid, songid, rating (1-5), daterated
// Supports: adding ratings, updating ratings, getting ratings for songs/users

using Models;                           // Rating model class
using System;                           // DateTime and other utilities
using System.Collections.Generic;        // Collections like List<T>
using System.Threading.Tasks;            // Async operations

namespace DBL
{
    /// <summary>
    /// RatingsDB - Database access class for song ratings
    /// Inherits from BaseDB to get common CRUD operations
    /// Handles the many-to-many relationship between users and songs via ratings
    /// </summary>
    public class RatingsDB : BaseDB<Rating>
    {
        /// <summary>
        /// Returns the database table name for ratings
        /// </summary>
        protected override string GetTableName() => "ratings";

        /// <summary>
        /// Returns the primary key column name
        /// </summary>
        protected override string GetPrimaryKeyName() => "ratingid";

        /// <summary>
        /// Converts a database row to a Rating model object
        /// Called by base class when reading from database
        /// </summary>
        /// <param name="row">Database row as object array</param>
        /// <returns>Rating model instance</returns>
        protected override async Task<Rating> CreateModelAsync(object[] row)
        {
            return new Rating(
                int.Parse(row[0].ToString()),   // ratingid - unique rating ID
                int.Parse(row[1].ToString()),   // userid - who gave the rating
                int.Parse(row[2].ToString()),   // songid - which song was rated
                int.Parse(row[3].ToString()),   // rating - star rating 1-5
                row[4] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row[4]) : null // daterated - when rated
            );
        }

        // ===== STANDARD CRUD METHODS =====

        /// <summary>
        /// Get all ratings from the database
        /// </summary>
        /// <returns>List of all ratings</returns>
        public async Task<List<Rating>> GetAllAsync()
        {
            return await SelectAllAsync();
        }

        /// <summary>
        /// Get a specific rating by its primary key (ratingid)
        /// </summary>
        /// <param name="ratingId">The rating ID to find</param>
        /// <returns>Rating object or null if not found</returns>
        public async Task<Rating?> GetRatingByPkAsync(int ratingId)
        {
            var p = new Dictionary<string, object> { { "ratingid", ratingId } };
            var list = await SelectAllAsync(p);
            return list.Count == 1 ? list[0] : null;
        }

        /// <summary>
        /// Add a new rating to the database
        /// Validates that rating is between 1-5 before inserting
        /// </summary>
        /// <param name="rating">Rating object to insert</param>
        /// <returns>The inserted rating with generated ID</returns>
        public async Task<Rating> InsertGetObjAsync(Rating rating)
        {
            // Validate rating value is within allowed range
            if (rating.Rate < 1 || rating.Rate > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            // Prepare data for insertion
            var values = new Dictionary<string, object>
            {
                { "userid", rating.UserId },     // Who gave the rating
                { "songid", rating.SongId },     // Which song was rated
                { "rating", rating.Rate }        // The rating value (1-5)
            };

            // Insert and return the created object
            return await base.InsertGetObjAsync(values);
        }

        /// <summary>
        /// Update an existing rating
        /// Validates rating value and updates the database
        /// </summary>
        /// <param name="rating">Rating object with updated values</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> UpdateAsync(Rating rating)
        {
            // Validate rating value
            if (rating.Rate < 1 || rating.Rate > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            // Data to update
            var values = new Dictionary<string, object> { { "rating", rating.Rate } };

            // Which record to update (by ratingid)
            var filter = new Dictionary<string, object> { { "ratingid", rating.RatingId } };

            // Perform update
            return await base.UpdateAsync(values, filter);
        }

        /// <summary>
        /// Delete a rating from the database
        /// </summary>
        /// <param name="rating">Rating object to delete</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> DeleteAsync(Rating rating)
        {
            var filter = new Dictionary<string, object> { { "ratingid", rating.RatingId } };
            return await base.DeleteAsync(filter);
        }

        // ===== QUERY METHODS FOR SPECIFIC DATA =====

        /// <summary>
        /// Get all ratings for a specific song
        /// Used to calculate average rating for display
        /// </summary>
        /// <param name="songId">ID of the song</param>
        /// <returns>List of all ratings for this song</returns>
        public async Task<List<Rating>> GetRatingsBySongAsync(int songId)
        {
            var p = new Dictionary<string, object> { { "songid", songId } };
            return await SelectAllAsync(p);
        }

        /// <summary>
        /// Alias for GetRatingsBySongAsync (for consistency with other methods)
        /// </summary>
        public async Task<List<Rating>> GetRatingsForSongAsync(int songId)
        {
            return await GetRatingsBySongAsync(songId);
        }

        /// <summary>
        /// Get all ratings given by a specific user
        /// Useful for user profile pages
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>List of all ratings by this user</returns>
        public async Task<List<Rating>> GetRatingsByUserAsync(int userId)
        {
            var p = new Dictionary<string, object> { { "userid", userId } };
            return await SelectAllAsync(p);
        }

        /// <summary>
        /// Get the rating that a specific user gave to a specific song
        /// Returns null if user hasn't rated this song
        /// Used to show user's current rating in the UI
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="songId">ID of the song</param>
        /// <returns>User's rating for this song, or null</returns>
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

        /// <summary>
        /// Alias for GetRatingByUserAndSongAsync (for consistency)
        /// </summary>
        public async Task<Rating?> GetUserRatingForSongAsync(int userId, int songId)
        {
            return await GetRatingByUserAndSongAsync(userId, songId);
        }

        /// <summary>
        /// Update a rating by providing ratingId and new rating value
        /// Alternative to UpdateAsync that takes a full Rating object
        /// </summary>
        /// <param name="ratingId">ID of the rating to update</param>
        /// <param name="newRating">New rating value (1-5)</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> UpdateRatingAsync(int ratingId, int newRating)
        {
            // Validate rating value
            if (newRating < 1 || newRating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            // Data to update
            var values = new Dictionary<string, object> { { "rating", newRating } };

            // Which record to update
            var filter = new Dictionary<string, object> { { "ratingid", ratingId } };

            // Perform update
            return await base.UpdateAsync(values, filter);
        }
    }
}
