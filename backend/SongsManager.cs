using DBL;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend
{
    public class SongsManager
    {
        // Database Services
        private SongsDB songsDB = new SongsDB();
        private SongArtistsDB songArtistsDB = new SongArtistsDB();
        private GenereDB genereDB = new GenereDB();

        // Data Collections
        public List<Song> Songs { get; set; } = new List<Song>();
        public List<Song> FilteredSongs { get; set; } = new List<Song>();
        public List<Genre> Genres { get; set; } = new List<Genre>();

        // User Management
        public Dictionary<int, string> UserNames { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> SongArtists { get; set; } = new Dictionary<int, string>();

        // Search & Upload
        public string SearchQuery { get; set; } = "";
        public string UploadTitle { get; set; } = "";
        public int UploadGenreId { get; set; } = 1;
        public string AdditionalArtists { get; set; } = "";
        public bool IsUploading { get; set; } = false;
        public string CurrentPlayingId { get; set; } = "";
        public bool IsLoading { get; set; } = true;

        // Load all songs
        public async Task LoadSongsAsync()
        {
            IsLoading = true;
            Songs = await songsDB.SelectAllAsync();
            FilteredSongs = new List<Song>(Songs);

            // Load user names for all songs
            await LoadUserNamesAsync();

            IsLoading = false;
        }

        // Load genres
        public async Task LoadGenresAsync()
        {
            Genres = await genereDB.GetAllAsync();
            if (Genres.Any() && UploadGenreId == 1)
            {
                UploadGenreId = Genres.First().genreID;
            }
        }

        // Search songs
        public async Task SearchSongs()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredSongs = new List<Song>(Songs);
            }
            else
            {
                FilteredSongs = await songsDB.SearchLikeAsync(SearchQuery);
            }
        }

        // Upload song
        public async Task<bool> UploadSongAsync(string fileName, Listener currentUser)
        {
            if (string.IsNullOrWhiteSpace(UploadTitle) || currentUser == null)
            {
                return false;
            }

            IsUploading = true;

            try
            {
                // Save to database
                var webPath = $"/uploads/{fileName}";
                var duration = 180; // Default 3 minutes

                var newSong = await songsDB.InsertSongAsync(UploadTitle, duration, webPath, currentUser.userid, UploadGenreId);

                // Add the uploader as the main artist
                await songArtistsDB.AddSongArtistAsync(newSong.SongId, currentUser.userid, "main");

                // Add additional artists if specified
                if (!string.IsNullOrWhiteSpace(AdditionalArtists))
                {
                    var artistUsernames = AdditionalArtists.Split(',')
                        .Select(name => name.Trim())
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToArray();

                    foreach (var username in artistUsernames)
                    {
                        try
                        {
                            var artistUser = await GetUserByUsernameAsync(username);
                            if (artistUser != null && artistUser.userid != currentUser.userid)
                            {
                                await songArtistsDB.AddSongArtistAsync(newSong.SongId, artistUser.userid, "featured");
                            }
                        }
                        catch
                        {
                            // Skip if user not found
                        }
                    }
                }

                // Reset form
                UploadTitle = "";
                UploadGenreId = Genres.Any() ? Genres.First().genreID : 1;
                AdditionalArtists = "";

                // Reload songs
                await LoadSongsAsync();

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                IsUploading = false;
            }
        }

        // Play song
        public void PlaySong(string songId)
        {
            CurrentPlayingId = songId;
        }

        // Format duration
        public string FormatDuration(int seconds)
        {
            var minutes = seconds / 60;
            var remainingSeconds = seconds % 60;
            return $"{minutes}:{remainingSeconds:D2}";
        }

        // Load user names
        private async Task LoadUserNamesAsync()
        {
            try
            {
                UserNames.Clear();
                SongArtists.Clear();

                // Get all unique user IDs
                var userIds = Songs.Select(s => s.UserId).Distinct().ToList();

                // Also get all artist IDs from song_artists table
                try
                {
                    foreach (var song in Songs)
                    {
                        var songArtistIds = await songArtistsDB.GetSongArtistsAsync(song.SongId);
                        userIds.AddRange(songArtistIds);
                    }
                }
                catch
                {
                    // Skip if error
                }

                userIds = userIds.Distinct().ToList();

                if (userIds.Any())
                {
                    ListenerDB listenerDB = new ListenerDB();
                    foreach (var userId in userIds)
                    {
                        try
                        {
                            var user = await listenerDB.GetListenerByPkAsync(userId);
                            if (user != null)
                            {
                                UserNames[userId] = user.username;
                            }
                        }
                        catch
                        {
                            UserNames[userId] = $"User #{userId}";
                        }
                    }
                }

                // Load artist names for all songs
                foreach (var song in Songs)
                {
                    try
                    {
                        var artistNames = await GetArtistNamesAsync(song.SongId);
                        SongArtists[song.SongId] = artistNames;
                    }
                    catch
                    {
                        SongArtists[song.SongId] = "Unknown Artist";
                    }
                }
            }
            catch
            {
                // Skip if error
            }
        }

        // Get artist names for song
        private async Task<string> GetArtistNamesAsync(int songId)
        {
            try
            {
                var artistIds = await songArtistsDB.GetSongArtistsAsync(songId);

                // If no artists, try to migrate
                if (artistIds.Count == 0)
                {
                    var song = Songs.FirstOrDefault(s => s.SongId == songId);
                    if (song != null)
                    {
                        try
                        {
                            await songArtistsDB.AddSongArtistAsync(songId, song.UserId, "main");
                            artistIds.Add(song.UserId);
                        }
                        catch
                        {
                            artistIds = await songArtistsDB.GetSongArtistsAsync(songId);
                        }
                    }
                }

                if (artistIds.Count == 0)
                {
                    return "Unknown Artist";
                }

                var artistNames = new List<string>();
                foreach (var artistId in artistIds)
                {
                    if (UserNames.ContainsKey(artistId))
                    {
                        artistNames.Add(UserNames[artistId]);
                    }
                    else
                    {
                        artistNames.Add($"User #{artistId}");
                    }
                }

                return string.Join(", ", artistNames);
            }
            catch
            {
                return "Unknown Artist";
            }
        }

        // Get user by username
        private async Task<Listener> GetUserByUsernameAsync(string username)
        {
            try
            {
                ListenerDB listenerDB = new ListenerDB();
                return await listenerDB.GetListenerByUsernameAsync(username);
            }
            catch
            {
                return null;
            }
        }
    }
}
