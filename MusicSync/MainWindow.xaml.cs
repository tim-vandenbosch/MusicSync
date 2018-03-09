using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NReco.VideoConverter;
using VideoLibrary;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MusicSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FolderBrowserDialog browseFolder;

        public MainWindow()
        {
            InitializeComponent();
            buttonSync.IsEnabled = false;
        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            browseFolder = new FolderBrowserDialog();
            browseFolder.ShowDialog();
            textBoxFolder.Text = browseFolder.SelectedPath;
        }

        private void PasteClipboard(object sender, RoutedEventArgs e)
        {
            buttonSync.IsEnabled = false;
            labelYoutubeVid.Content = "";
            labelYoutubeFullName.Content = "";
            textBoxYoutubeUrl.Text = "";
            if (Clipboard.ContainsText())
            {
                if (CheckValidUrl(Clipboard.GetText()))
                {
                    textBoxUrl.Text = Clipboard.GetText();
                }
                else
                {
                    System.Windows.MessageBox.Show("There was no valid url found on your clipboard.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("There was no text found on your clipboard.");
            }
        }

        private bool CheckValidUrl(string url)
        {
            try
            {
                var req = WebRequest.Create(url);
                var res = req.GetResponse();
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

        private void SyncFolderToPlaylist(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Starting up service.");
            using (var service = Client.For(YouTube.Default))
            {
                System.Windows.MessageBox.Show("getting storage folder and playlist.");
                var url = textBoxUrl.Text;
                var folder = textBoxFolder.Text;
                Directory.CreateDirectory(folder);
                Directory.SetCurrentDirectory(folder);
                System.Windows.MessageBox.Show($"Local folder = {Directory.GetCurrentDirectory()}.");
                

                //var youtube = YouTube.Default;
                System.Windows.MessageBox.Show("getting vid.");
                //var vid = await youtube.GetVideoAsync(url);
                try
                {
                    var vid = service.GetVideo(url);
                    System.Windows.MessageBox.Show($"Found video = {vid.Title}.");
                    System.Windows.MessageBox.Show("attempting to save the vid.");
                    
                    File.WriteAllBytes(Directory.GetCurrentDirectory() +"\\"+ vid.FullName, vid.GetBytes());
                    System.Windows.MessageBox.Show("Video saved.");
                    var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                    ffMpeg.ConvertMedia(Directory.GetCurrentDirectory() + "\\" + vid.FullName, Directory.GetCurrentDirectory() + "\\" + "extracted_audio_" + vid.FullName, Format.ac3);
                }
                catch (UnauthorizedAccessException UAEx)
                {
                    System.Windows.MessageBox.Show("No acces to the folder.");
                }
            }
        }

        private void ValidateUrl(object sender, RoutedEventArgs e)
        {
            using (var service = Client.For(YouTube.Default))
            {
                var vid = service.GetVideo(textBoxUrl.Text);
                labelYoutubeVid.Content = vid.Title;
                labelYoutubeFullName.Content = vid.FullName;
                try
                {
                    textBoxYoutubeUrl.Text = vid.Uri;
                    buttonSync.IsEnabled = true;
                }
                catch (Exception exception)
                {
                    textBoxYoutubeUrl.Text = "[ERROR] url couldn't be validated!";
                    buttonSync.IsEnabled = false;
                }
            }
        }
    }
}
