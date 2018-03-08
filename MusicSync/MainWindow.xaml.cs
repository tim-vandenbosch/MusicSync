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
using VideoLibrary;
using Clipboard = System.Windows.Clipboard;
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
        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            browseFolder = new FolderBrowserDialog();
            browseFolder.ShowDialog();
            textBoxFolder.Text = browseFolder.SelectedPath;
        }

        private void PasteClipboard(object sender, RoutedEventArgs e)
        {
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
            var url = textBoxUrl.Text;
            var folder = textBoxFolder.Text;

            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(url);
            File.WriteAllBytes(folder, vid.GetBytes());
        }
    }
}
