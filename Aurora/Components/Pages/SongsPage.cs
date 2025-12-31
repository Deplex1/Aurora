// =================================================================================
// SongsPage.cs - COMPLETE BACKEND LOGIC FOR SONGS MANAGEMENT PAGE
// =================================================================================
// This file contains all the C# backend logic for the Songs.razor page.
// NO dependency injection, NO advanced features, basic high-school level code.
// Handles: Song loading, ratings, file uploads, audio controls, UI state management.

using Models;                           // Import data models (Song, Rating, Listener classes)
using DBL;                              // Import database layer (SongsDB, RatingsDB classes)
using Microsoft.AspNetCore.Components.Forms;  // For file upload handling (IBrowserFile)
using System.IO;                        // For file system operations (saving uploaded files)
using Microsoft.AspNetCore.Components;   // Base class for Blazor components
using Microsoft.AspNetCore.Http;         // HTTP utilities (though not heavily used)
using System.Threading.Tasks;            // Async programming support
using System.Collections.Generic;        // Collections like List<T> and Dictionary<TKey,TValue>
using System.Linq;                       // LINQ for data querying and manipulation
using System.Timers;                     // For Timer class to simulate audio playback
using System;                            // For Math functions

namespace Aurora.Components.Pages
{
    /// <summary>
    /// SongsPage - Main backend class that handles all songs-related functionality
    /// Inherits from ComponentBase to get Blazor component lifecycle methods
    /// Implements IDisposable to clean up timer resources
    /// This is a PARTIAL class - the UI part is in Songs.razor
    /// </summary>
    public partial class SongsPage : ComponentBase, IDisposable
    {
        // ===== DATA STORAGE PROPERTIES =====
        /// <summary>
        /// List of all songs available to the current user
        /// Populated from database when page loads
        /// </summary>
        protected List<Song> userSongs = new List<Song>();

        /// <summary>
        /// Information about the currently logged-in user
        /// Used for filtering songs and rating operations
        /// </summary>
        protected Listener currentUser;

        /// <summary>
        /// Maps song IDs to lists of artist names
        /// Key: songId, Value: List of artist names for that song
        /// Currently simplified - in future could support many-to-many artists
        /// </summary>
        protected Dictionary<int, List<string>> songArtists = new Dictionary<int, List<string>>();

        /// <summary>
        /// Maps song IDs to their average rating (calculated from all user ratings)
        /// Key: songId, Value: average rating (0.0 to 5.0)
        /// </summary>
        protected Dictionary<int, double> songRatings = new Dictionary<int, double>();

        /// <summary>
        /// Maps song IDs to the current user's personal rating
        /// Key: songId, Value: user's rating (1-5, or missing if not rated)
        /// </summary>
        protected Dictionary<int, int> userRatings = new Dictionary<int, int>();

        // ===== FILE UPLOAD PROPERTIES =====
        /// <summary>
        /// The audio file selected by user for upload
        /// Set when user chooses file in upload form
        /// </summary>
        protected IBrowserFile uploadFile;

        /// <summary>
        /// Title of the song being uploaded
        /// User enters this in the upload form
        /// </summary>
        protected string songTitle = "";

        /// <summary>
        /// Primary artist name for the song
        /// User enters this in the upload form
        /// </summary>
        protected string primaryArtist = "";

        /// <summary>
        /// Additional artists (comma-separated)
        /// User enters this in the upload form
        /// </summary>
        protected string additionalArtists = "";

        /// <summary>
        /// Selected genre ID for the song
        /// User chooses from dropdown in upload form
        /// </summary>
        protected int selectedGenreId = 1;

        // ===== AUDIO PLAYBACK PROPERTIES =====
        /// <summary>
        /// File path of the currently playing song
        /// Used to track which song is active
        /// </summary>
        protected string currentPlayingSong = "";

        /// <summary>
        /// Whether audio is currently playing or paused
        /// Controls play/pause button appearance
        /// </summary>
        protected bool isPlaying = false;

        /// <summary>
        /// Current playback time in seconds (not display format)
        /// Used for progress calculations
        /// </summary>
        protected int currentTimeSeconds = 0;

        /// <summary>
        /// Current playback time in "MM:SS" format (for display)
        /// Updated by the playback timer
        /// </summary>
        protected string currentTime = "0:00";

        /// <summary>
        /// Total duration of current song in "MM:SS" format
        /// Calculated from song.Duration field
        /// </summary>
        protected string totalDuration = "0:00";

        /// <summary>
        /// Total duration in seconds for progress calculations
        /// </summary>
        protected int totalDurationSeconds = 0;

        /// <summary>
        /// Timer for simulating audio playback
        /// Updates progress every second when playing
        /// </summary>
        private System.Timers.Timer playbackTimer;

        // ===== CONSTRUCTOR =====
        /// <summary>
        /// Initialize the playback timer
        /// </summary>
        public SongsPage()
        {
            // Create timer that fires every 1 second
            playbackTimer = new System.Timers.Timer(1000); // 1000ms = 1 second
            playbackTimer.Elapsed += OnPlaybackTimerElapsed;
            playbackTimer.AutoReset = true;
        }

        // ===== TIMER EVENT HANDLER =====
        /// <summary>
        /// Called every second when audio is playing
        /// Updates the progress bar and current time display
        /// </summary>
        private void OnPlaybackTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Safety check - make sure we have a valid duration
            if (totalDurationSeconds <= 0)
            {
                playbackTimer.Stop();
                return;
            }

            // Increment current time by 1 second
            currentTimeSeconds++;

            // Check if we've reached the end of the song
            if (currentTimeSeconds >= totalDurationSeconds)
            {
                // ===== SONG FINISHED =====
                // Stop the timer and reset playback state safely
                playbackTimer.Stop();
                isPlaying = false;
                currentTimeSeconds = 0; // Reset to beginning
                currentTime = "0:00";

                // Force UI update on the main thread
                InvokeAsync(() => StateHasChanged());
                return; // Exit early, don't update time display again
            }

            // Update the time display for normal playback
            currentTime = FormatDuration(currentTimeSeconds);

            // Force UI update on the main thread
            InvokeAsync(() => StateHasChanged());
        }

        // ===== UI STATE PROPERTIES =====
        /// <summary>
        /// Controls whether the upload modal dialog is visible
        /// Set to true when user clicks "Upload Song" button
        /// </summary>
        protected bool showUploadModal = false;

        // ===== BLAZOR LIFECYCLE METHODS =====
        /// <summary>
        /// Called after the component has finished rendering
        /// This is where we initialize data on first load
        /// </summary>
        /// <param name="firstRender">True if this is the first time component rendered</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Only run initialization on first render to avoid reloading data
            if (firstRender)
            {
                // Start the authentication and data loading process
                await HandleUserAuthentication();
            }
        }

        // ===== AUTHENTICATION & INITIALIZATION =====
        /// <summary>
        /// Handles user authentication and initial data loading
        /// In a real app, this would check login status and get user from session
        /// For demo purposes, creates a test user
        /// </summary>
        private async Task HandleUserAuthentication()
        {
            // Create a demo user for testing purposes
            // In real app: check session/cookies for logged-in user
            currentUser = new Listener
            {
                userid = 1,                    // Unique user ID from database
                username = "demo_user",        // User's display name
                email = "demo@example.com"     // User's email address
            };

            // After user is set, load all their songs and related data
            await LoadSongs();
        }

        // ===== DATA LOADING METHODS =====
        /// <summary>
        /// Loads all songs from database and related data (artists, ratings)
        /// This is called when page first loads and after uploading new songs
        /// </summary>
        private async Task LoadSongs()
        {
            try
            {
                // Create database connection object
                SongsDB songsDB = new SongsDB();

                // Get all songs from database
                // In real app: filter by currentUser.userid to show only user's songs
                userSongs = await songsDB.SelectAllAsync();

                // OPTIONAL: Uncomment to show only current user's songs
                // userSongs = userSongs.Where(s => s.UserId == currentUser.userid).ToList();

                // Load additional data for each song
                await LoadSongArtists();    // Load artist information
                await LoadSongRatings();    // Load rating information
            }
            catch
            {
                // If database error, just show empty list
                // In real app: show error message to user
                userSongs = new List<Song>();
            }
        }

        /// <summary>
        /// Loads artist information for each song
        /// Currently simplified - just shows "Unknown Artist" for all songs
        /// Future: implement many-to-many relationship with artists table
        /// </summary>
        private async Task LoadSongArtists()
        {
            // Clear previous data
            songArtists.Clear();

            // For each song, add a default artist name
            // In real implementation: query song_artists junction table
            foreach (var song in userSongs)
            {
                // Create list with one artist name
                songArtists[song.SongId] = new List<string> { "Unknown Artist" };
            }
        }

        /// <summary>
        /// Loads rating information for all songs
        /// Calculates average ratings and loads current user's ratings
        /// Updates songRatings and userRatings dictionaries
        /// </summary>
        private async Task LoadSongRatings()
        {
            try
            {
                // Clear previous rating data
                songRatings.Clear();
                userRatings.Clear();

                // Create database connection for ratings
                RatingsDB ratingsDB = new RatingsDB();

                // Process each song to calculate ratings
                foreach (var song in userSongs)
                {
                    // ===== CALCULATE AVERAGE RATING =====
                    // Get all ratings for this specific song
                    var songRatingsList = await ratingsDB.GetRatingsForSongAsync(song.SongId);

                    if (songRatingsList.Any())  // If there are any ratings
                    {
                        // Calculate average: sum of all ratings divided by number of ratings
                        // LINQ Average() function does this calculation
                        songRatings[song.SongId] = songRatingsList.Average(r => r.Rate);
                    }
                    else
                    {
                        // No ratings yet, set to 0
                        songRatings[song.SongId] = 0;
                    }

                    // ===== GET CURRENT USER'S RATING =====
                    // Check if current user has rated this song
                    var userRating = await ratingsDB.GetUserRatingForSongAsync(currentUser.userid, song.SongId);
                    if (userRating != null)
                    {
                        // User has rated this song, store their rating (1-5)
                        userRatings[song.SongId] = userRating.Rate;
                    }
                    // If user hasn't rated, dictionary stays empty for this song
                }
            }
            catch
            {
                // Handle rating loading errors silently
                // In real app: log error and/or show message to user
            }
        }

        // ===== FILE UPLOAD METHODS =====
        /// <summary>
        /// Handles the song upload process when user submits the upload form
        /// Saves file to disk, creates database record, refreshes UI
        /// </summary>
        protected async Task HandleUpload()
        {
            // Validate required fields before processing
            if (uploadFile == null || string.IsNullOrEmpty(songTitle))
                return;  // Exit if no file or no title

            try
            {
                // ===== STEP 1: SAVE FILE TO DISK =====
                // Create unique filename to avoid conflicts
                // Format: GUID + original filename
                var fileName = $"{Guid.NewGuid()}_{uploadFile.Name}";

                // Build full path: wwwroot/audio/filename.mp3
                var filePath = Path.Combine("wwwroot", "audio", fileName);

                // Ensure the audio directory exists
                Directory.CreateDirectory(Path.Combine("wwwroot", "audio"));

                // Save the uploaded file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadFile.OpenReadStream().CopyToAsync(stream);
                }

                // ===== STEP 2: GET SONG DURATION =====
                // In real app: use audio library to read actual duration from file
                // For demo: use default 3 minutes (180 seconds)
                int duration = 180;

                // ===== STEP 3: SAVE TO DATABASE =====
                // Create database connection and insert new song record
                SongsDB songsDB = new SongsDB();
                Song newSong = await songsDB.InsertSongAsync(
                    songTitle,      // Song title from form
                    duration,       // Duration in seconds
                    filePath,       // File path on disk
                    currentUser.userid,  // ID of user uploading
                    selectedGenreId     // Selected genre ID
                );

                // ===== STEP 4: RESET FORM =====
                // Clear all form fields
                uploadFile = null;
                songTitle = "";
                primaryArtist = "";
                additionalArtists = "";
                showUploadModal = false;  // Close the modal

                // ===== STEP 5: REFRESH UI =====
                // Reload all songs to show the new one
                await LoadSongs();

                // Tell Blazor to re-render the UI
                StateHasChanged();
            }
            catch
            {
                // Handle upload errors (file save, database insert, etc.)
                // In real app: show error message to user
            }
        }

        // ===== RATING SYSTEM METHODS =====
        /// <summary>
        /// Handles when user clicks on a star to rate a song
        /// Either creates new rating or updates existing one
        /// </summary>
        /// <param name="songId">ID of the song being rated</param>
        /// <param name="rating">Rating value (1-5 stars)</param>
        protected async Task RateSong(int songId, int rating)
        {
            // Validate rating is within allowed range
            if (rating < 1 || rating > 5) return;

            try
            {
                // Create database connection for ratings
                RatingsDB ratingsDB = new RatingsDB();

                // Check if user has already rated this song
                var existingRating = await ratingsDB.GetUserRatingForSongAsync(currentUser.userid, songId);

                if (existingRating != null)
                {
                    // ===== UPDATE EXISTING RATING =====
                    // User already rated this song, just update the value
                    existingRating.Rate = rating;
                    await ratingsDB.UpdateAsync(existingRating);
                }
                else
                {
                    // ===== CREATE NEW RATING =====
                    // First time user is rating this song
                    Rating newRating = new Rating(
                        0,                          // ratingId (0 = auto-generated)
                        currentUser.userid,         // user who is rating
                        songId,                     // song being rated
                        rating,                     // rating value 1-5
                        DateTime.Now               // timestamp
                    );
                    await ratingsDB.InsertGetObjAsync(newRating);
                }

                // Refresh rating data and update UI
                await LoadSongRatings();
                StateHasChanged();
            }
            catch
            {
                // Handle rating errors (database issues, etc.)
                // In real app: show error message to user
            }
        }

        // ===== UTILITY METHODS =====
        /// <summary>
        /// Converts seconds to "MM:SS" format for display
        /// Example: 125 seconds -> "2:05"
        /// </summary>
        /// <param name="seconds">Duration in seconds</param>
        /// <returns>Formatted time string</returns>
        protected string FormatDuration(int seconds)
        {
            // Calculate minutes (integer division)
            int minutes = seconds / 60;

            // Calculate remaining seconds (modulo operation)
            int remainingSeconds = seconds % 60;

            // Format as "M:SS" with leading zero for seconds
            return $"{minutes}:{remainingSeconds:D2}";
        }

        // ===== AUDIO CONTROL METHODS =====
        /// <summary>
        /// Called when user clicks play button on a song
        /// Starts the playback timer and updates state
        /// </summary>
        /// <param name="songPath">File path of the song to play</param>
        protected void PlaySong(string songPath)
        {
            // Stop any currently playing timer first
            if (playbackTimer.Enabled)
            {
                playbackTimer.Stop();
            }

            // Find the song object to get its duration
            var song = userSongs.FirstOrDefault(s => s.FilePath == songPath);
            if (song != null)
            {
                // Set song duration for progress calculations
                totalDurationSeconds = song.Duration;
                totalDuration = FormatDuration(song.Duration);
            }
            else
            {
                // Song not found, don't start playback
                return;
            }

            // Set which song is currently playing
            currentPlayingSong = songPath;

            // Update playback state
            isPlaying = true;

            // Start the playback timer
            playbackTimer.Start();

            // Refresh UI to show play/pause button changes
            StateHasChanged();
        }

        /// <summary>
        /// Called when user clicks pause button
        /// Stops the playback timer and updates state
        /// </summary>
        protected void PauseSong()
        {
            // Update playback state
            isPlaying = false;

            // Stop the playback timer
            playbackTimer.Stop();

            // Refresh UI to show play button
            StateHasChanged();
        }

        // ===== PROGRESS CALCULATION =====
        /// <summary>
        /// Calculates the current progress percentage for the progress bar
        /// </summary>
        /// <returns>Progress percentage (0-100)</returns>
        protected double GetProgressPercentage()
        {
            if (totalDurationSeconds == 0) return 0;
            return (double)currentTimeSeconds / totalDurationSeconds * 100;
        }

        // ===== UI CONTROL METHODS =====
        /// <summary>
        /// Shows the upload modal dialog
        /// Called when user clicks "Upload Song" button
        /// </summary>
        protected void ShowUploadForm()
        {
            showUploadModal = true;
            StateHasChanged();
        }

        /// <summary>
        /// Hides the upload modal dialog
        /// Called when user clicks "Cancel" or after successful upload
        /// </summary>
        protected void HideUploadForm()
        {
            showUploadModal = false;
            StateHasChanged();
        }

        /// <summary>
        /// Handles when user selects a file in the upload form
        /// Stores the selected file for later processing
        /// </summary>
        /// <param name="e">File selection event arguments</param>
        protected void HandleFileSelection(InputFileChangeEventArgs e)
        {
            // Store the selected file for upload processing
            uploadFile = e.File;
        }

        // ===== UI EVENT WRAPPER METHODS =====
        // These methods exist because Blazor onclick events can't directly call async methods
        // They wrap the async calls in fire-and-forget pattern

        /// <summary>
        /// Async wrapper for RateSong - allows onclick to call async method properly
        /// Used by star rating buttons in UI
        /// </summary>
        /// <param name="songId">ID of song being rated</param>
        /// <param name="rating">Rating value (1-5)</param>
        protected async Task RateSongAsync(int songId, int rating)
        {
            await RateSong(songId, rating);
        }

        /// <summary>
        /// Wrapper for play/pause functionality
        /// Toggles between play and pause based on current state
        /// If switching songs, resets progress to beginning
        /// </summary>
        /// <param name="songPath">File path of the song</param>
        protected void PlayPauseSong(string songPath)
        {
            // Check if this song is currently playing
            if (isPlaying && currentPlayingSong == songPath)
            {
                // Same song is playing, so pause it
                PauseSong();
            }
            else
            {
                // Different song or nothing playing
                if (currentPlayingSong != songPath)
                {
                    // Switching to a different song - reset progress
                    currentTimeSeconds = 0;
                    currentTime = "0:00";
                }

                // Play this song
                PlaySong(songPath);
            }
        }

        // ===== COMPONENT LIFECYCLE =====
        /// <summary>
        /// Clean up resources when component is disposed
        /// Prevents memory leaks from timer
        /// </summary>
        public void Dispose()
        {
            if (playbackTimer != null)
            {
                playbackTimer.Stop();
                playbackTimer.Dispose();
            }
        }

        // ===== SEEKING FUNCTIONALITY =====
        /// <summary>
        /// Handles seeking to a position in the audio timeline by clicking on the progress bar
        /// Calculates the click position and updates the current playback time accordingly
        /// </summary>
        /// <param name="e">Mouse event containing click coordinates</param>
        /// <param name="songDuration">Total duration of the song in seconds</param>
        protected void SeekToPosition(Microsoft.AspNetCore.Components.Web.MouseEventArgs e, int songDuration)
        {
            // Approximate width of the progress container in pixels
            // In a real implementation, this could be calculated dynamically
            const double containerWidth = 200.0; // pixels

            // Get the X offset where the user clicked within the container
            var offsetX = e.OffsetX;

            // Calculate the relative position where clicked (0.0 to 1.0)
            var clickRatio = offsetX / containerWidth;

            // Clamp the ratio to valid range (0.0 to 1.0)
            clickRatio = Math.Max(0.0, Math.Min(1.0, clickRatio));

            // Convert to time position in seconds
            var seekTimeSeconds = (int)(clickRatio * songDuration);

            // Update the current playback time
            currentTimeSeconds = seekTimeSeconds;
            currentTime = FormatDuration(currentTimeSeconds);

            // Force UI update to reflect the seek
            StateHasChanged();
        }
    }
}
