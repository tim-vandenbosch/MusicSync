using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using MusicSync.objects;
using MusicSync.Properties;
using NReco.VideoConverter;
using VideoLibrary;
using WrapYoutubeDl;
using Video = MusicSync.objects.Video;

namespace MusicSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.FolderBrowserDialog _browseFolder;
        private string _folder = Settings.Default.WantedFolder;
        private string _ytdl = "binary/youtube-dl.exe";
        private Playlist _playList;
        private List<Song> _songList;

        public MainWindow()
        {
            InitializeComponent();
            buttonSync.IsEnabled = true;
            textBoxFolder.Text = _folder;
            _songList = new List<Song>();
        }

        /// <summary>
        /// When looking for a local folder on the pc to store the playlist to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            _browseFolder = new System.Windows.Forms.FolderBrowserDialog();
            _browseFolder.ShowDialog();
            textBoxFolder.Text = _browseFolder.SelectedPath;
            Settings.Default.WantedFolder = _browseFolder.SelectedPath;
            Settings.Default.Save();
        }

        /// <summary>
        /// Pasting the link from the browser without using shortcuts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteClipboard(object sender, RoutedEventArgs e)
        {
            buttonSync.IsEnabled = false;
            if (Clipboard.ContainsText())
            {
                if (Clipboard.ContainsText()) textBoxUrl.Text = Clipboard.GetText();
                CheckValidUrl(textBoxUrl.Text);
                buttonSync.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("There was no text found on your clipboard.");
            }
        }

        /// <summary>
        /// Checking of the url is actually is an url and if it's from youtube.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool CheckValidUrl(string url)
        {
            try
            {
                var req = WebRequest.Create(url);
                var res = req.GetResponse();
                res.Close();
                using (var service = Client.For(YouTube.Default))
                {
                    var vid = service.GetVideo(textBoxUrl.Text);
                }
                return true;
            }
            catch (WebException)
            {
                return false;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Syncing the youtubePlaylist with the chosen local folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncFolderToPlaylist(object sender, RoutedEventArgs e)
        {
            // Where to download to
            SetCurrentFolder();

            // Where the video comes from
            var url = textBoxUrl.Text;
            if (!CheckValidUrl(url)) return;
            DownloadOneVideoAndConvertIt(url);
        }

        private void DownloadOneVideoAndConvertIt(string url)
        {
            // Getting info from video
            Console.WriteLine("Starting up service.");
            string title;
            using (var service = Client.For(YouTube.Default))
            {
                // getting info
                Console.WriteLine("Getting video information.");
                var vid = service.GetVideo(url);
                Console.WriteLine($"Found video = {vid.Title}.");
                title = vid.Title.Replace('#', ' ').Replace('|', ' ');
                // Console.WriteLine($"Video url: {vid.Uri}.");
            }

            // initiating download
            var wantedVideoFile = $"{title}.webm";
            var folder = _folder.Replace("\\", "/");
            var arguments = string.Format($"--continue --no-playlist --no-overwrites --restrict-filenames --extract-audio --audio-format mp3 {url} -o \"{folder}/{wantedVideoFile}\"");  //--ignore-errors
            YoutubeInteraction(arguments);

            // initiating convert to mp3
            var wantedAudioFile = $"{title}.mp3";
            Console.WriteLine("Starting up ffmpeg on \"below normal\" priority."); // Making sure ffmpeg doesn't suck your computer dry
            var ffMpeg = new FFMpegConverter { FFMpegProcessPriority = ProcessPriorityClass.BelowNormal };
            Console.WriteLine("ffmpeg Created and converting media.");
            ffMpeg.ConvertMedia(_folder + "\\" + wantedVideoFile,
                                _folder + "\\" + wantedAudioFile,
                                AudioFormat.Mp3.ToString());
            Console.WriteLine("Media converted.");

            // deleting vids
            Console.WriteLine("Deleting video.");
            File.Delete(_folder + "\\" + wantedVideoFile);
        }

        /// <summary>
        /// Opening the selected folder for syncing, in file explorer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenSelectedFolder(object sender, RoutedEventArgs e)
        {
            SetCurrentFolder();
            Process.Start(_folder);
        }

        /// <summary>
        /// Selecting the folder for use.
        /// </summary>
        private void SetCurrentFolder()
        {
            _folder = textBoxFolder.Text;
            Directory.CreateDirectory(_folder);
            Console.WriteLine($"Setting local folder = {_folder}.");
        }

        /// <summary>
        /// The downloader of youtube files
        /// </summary>
        /// <param name="url"></param>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        private void YoutubeInteraction(string arguments)
        {
            Console.WriteLine("Setting up downloader.");

            // Method 2
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Console.WriteLine($"{Directory.GetCurrentDirectory()}\\{_ytdl}".Replace("\\", "/"));
            process.StartInfo.FileName = $"{Directory.GetCurrentDirectory()}\\{_ytdl}".Replace("\\", "/");
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;

            Console.WriteLine("Starting download");
            process.Start();

            while (process.HasExited == false)
            {
                Console.WriteLine(process.StandardOutput.ReadLine());
                // Console.WriteLine("Waiting for download to complete.");
                System.Threading.Thread.Sleep(1000);                   // wait while process exits;
            }
            Console.WriteLine("Download complete!");
        }

        /// <summary>
        /// Display a found video on youtube with information
        /// </summary>
        /// <param name="__playlist"></param>
        private void CreateXamlItem(Playlist __playlist)
        {
            var dataitem = DataGrid.DataContext as objects.Video;
            // dataitem.Title;
            
        }

        /// <summary>
        /// Checks if the url for youtube playlist
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool CheckPlaylistUrl(Uri url)
        {
            return url.Host == "youtube";
        }

        /// <summary>
        /// Gets all the videos-data and sets them to objects
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<objects.Video> CreatePlaylist(Uri url)
        {
            List<Video> foundVideos = new List<Video>();
            if (CheckPlaylistUrl(url))
            {
                //TODO: get songs from youtube
                // foreach found song, add to foundVideo
                return foundVideos;
            }
            else
            {
                // if just one song found, still add to playlist
                // foundVideos.Add();
                return foundVideos;
            }
        }
    }
}
