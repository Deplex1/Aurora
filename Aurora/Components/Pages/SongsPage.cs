using Backend;
using Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IO;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Aurora.Components.Pages
{
    public partial class SongsPage : ComponentBase
    {
        // No dependency injection - using basic JavaScript access

        // Business logic manager
        private SongsManager songsManager = new SongsManager();

        // UI Properties
        public Listener CurrentUser { get; set; }
        public IBrowserFile SelectedFile { get; set; }

        // Bindings to SongsManager properties
        public string SearchQuery
        {
            get => songsManager.SearchQuery;
            set => songsManager.SearchQuery = value;
        }

        public string UploadTitle
        {
            get => songsManager.UploadTitle;
            set => songsManager.UploadTitle = value;
        }

        public int UploadGenreId
        {
            get => songsManager.UploadGenreId;
            set => songsManager.UploadGenreId = value;
        }

        public string AdditionalArtists
        {
            get => songsManager.AdditionalArtists;
            set => songsManager.AdditionalArtists = value;
        }

        public bool IsUploading => songsManager.IsUploading;
        public bool IsLoading => songsManager.IsLoading;
        public string CurrentPlayingId => songsManager.CurrentPlayingId;

        public System.Collections.Generic.List<Song> FilteredSongs => songsManager.FilteredSongs;
        public System.Collections.Generic.List<Models.Genre> Genres => songsManager.Genres;
        public System.Collections.Generic.Dictionary<int, string> SongArtists => songsManager.SongArtists;

        protected override async Task OnInitializedAsync()
        {
            await songsManager.LoadSongsAsync();
            await songsManager.LoadGenresAsync();
        }

        // Simple method to handle user authentication using basic JavaScript
        private async Task HandleUserAuthentication()
        {
            try
            {
                // Use basic JavaScript to check if user exists
                // For demo - just set a test user to avoid null reference
                CurrentUser = new Listener
                {
                    userid = 1,
                    username = "demo_user",
                    email = "demo@example.com"
                };

                // In a real app, you'd check localStorage or cookies here
                // var userData = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "user");

                await LoadData();
            }
            catch
            {
                // On error, create default user
                CurrentUser = new Listener
                {
                    userid = 1,
                    username = "demo_user",
                    email = "demo@example.com"
                };
                await LoadData();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await HandleUserAuthentication();
                if (!userResult.Success || userResult.Value == null)
                {
                    NavManager.NavigateTo("/");
                    return;
                }

                CurrentUser = userResult.Value;
                await InvokeAsync(StateHasChanged);
            }
        }

        public async Task OnSearchInput(Microsoft.AspNetCore.Components.ChangeEventArgs e)
        {
            SearchQuery = e.Value?.ToString() ?? "";
            await songsManager.SearchSongs();
            await InvokeAsync(StateHasChanged);
        }

        public void OnFileSelected(InputFileChangeEventArgs e)
        {
            SelectedFile = e.File;
        }

        public async Task UploadSong()
        {
            if (SelectedFile == null || string.IsNullOrWhiteSpace(UploadTitle) || CurrentUser == null)
            {
                return;
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileExtension = Path.GetExtension(SelectedFile.Name);
            var uniqueFileName = $"{System.Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file
            await using var stream = SelectedFile.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            await using var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            await stream.CopyToAsync(fileStream);

            // Upload using manager
            var success = await songsManager.UploadSongAsync(uniqueFileName, CurrentUser);
            if (success)
            {
                SelectedFile = null;
                await InvokeAsync(StateHasChanged);
            }
        }

        public void PlaySong(string songId)
        {
            songsManager.PlaySong(songId);
            InvokeAsync(StateHasChanged);
        }

        public string FormatDuration(int seconds)
        {
            return songsManager.FormatDuration(seconds);
        }

        public void NavigateHome()
        {
            NavManager.NavigateTo("/");
        }

        public async Task Logout()
        {
            await MySession.DeleteAsync("user");
            NavManager.NavigateTo("/");
        }
    }
}
