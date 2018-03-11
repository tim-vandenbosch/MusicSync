using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using NReco.VideoConverter;
using VideoLibrary;

namespace MusicSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.FolderBrowserDialog _browseFolder;
        private string _folder;

        public MainWindow()
        {
            InitializeComponent();
            buttonSync.IsEnabled = false;
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
        }

        /// <summary>
        /// Pasting the link from the browser without using shortcuts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteClipboard(object sender, RoutedEventArgs e)
        {
            buttonSync.IsEnabled = false;
            labelYoutubeVid.Content = "";
            labelYoutubeFullName.Content = "";
            labelYoutubeUrl.Content = "";
            if (Clipboard.ContainsText())
            {
                if (CheckValidUrl(Clipboard.GetText()))
                {
                    textBoxUrl.Text = Clipboard.GetText();
                }
                else
                {
                    MessageBox.Show("There was no valid url found on your clipboard.");
                }
            }
            else
            {
                MessageBox.Show("There was no text found on your clipboard.");
            }
        }

        /// <summary>
        /// Checking of the url is actually is an url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool CheckValidUrl(string url)
        {
            try
            {
                var req = WebRequest.Create(url);
                var res = req.GetResponse();
                if (res.ResponseUri.Host == "Youtube")
                {
                    res.Close();
                    return true;
                }
                else
                {
                    MessageBox.Show("The given link is not a youtube link.");
                    res.Close();
                    return false;
                }
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
            Console.WriteLine("Starting up service.");
            using (var service = Client.For(YouTube.Default))
            {
                Console.WriteLine("getting storage folder and playlist.");
                var url = textBoxUrl.Text;
                SetCurrentFolder();
                Console.WriteLine($"Local folder = {Directory.GetCurrentDirectory()}.");
                try
                {
                    Console.WriteLine("Getting video information.");
                    var vid = service.GetVideo(url);
                    Console.WriteLine($"Found video = {vid.Title}.");
                    Console.WriteLine($"Video url: {vid.Uri}.");
                    Console.WriteLine("Downloading video.");
                    var stream = vid.GetBytes();
                    Console.WriteLine("attempting to save the vid.");
                    File.WriteAllBytes(Directory.GetCurrentDirectory() +"\\"+ vid.FullName, stream);
                    Console.WriteLine("Video saved.");
                    Console.WriteLine("Starting up ffmpeg on \"below normal\" priority.");
                    // Making sure ffmpeg doesn't suck your computer dry
                    var ffMpeg = new FFMpegConverter {FFMpegProcessPriority = ProcessPriorityClass.BelowNormal};
                    Console.WriteLine("ffmpeg Created and converting media.");
                    ffMpeg.ConvertMedia(Directory.GetCurrentDirectory() + "\\" + vid.FullName, Directory.GetCurrentDirectory() + "\\" + "extracted_audio_" + vid.Title + ".mp3", AudioFormat.Mp3.ToString());
                    Console.WriteLine("Media converted.");

                    Console.WriteLine("Deleting video.");
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + vid.FullName);

                    MessageBox.Show($"Download and convertion complete for vid: {vid.Title}.");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show($"No acces to the folder: {_folder}.");
                }
            }
        }

        /// <summary>
        /// Validating visualy for the user if the youtube vid is able to be downloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateUrl(object sender, RoutedEventArgs e)
        {
            using (var service = Client.For(YouTube.Default))
            {
                var vid = service.GetVideo(textBoxUrl.Text);
                labelYoutubeVid.Content = vid.Title;
                labelYoutubeFullName.Content = vid.FullName;
                try
                {
                    // textBoxYoutubeUrl.Text = vid.Uri;
                    labelYoutubeUrl.Content = "url validated!";
                    buttonSync.IsEnabled = true;
                }
                catch (Exception)
                {
                    labelYoutubeUrl.Content = "[ERROR] url couldn't be validated!";
                    buttonSync.IsEnabled = false;
                }
            }
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
            Directory.SetCurrentDirectory(_folder);
            Console.WriteLine($"Opening local folder = {Directory.GetCurrentDirectory()}.");
        }
    }
}
