using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSync.objects
{
    public class Video
    {
        public Video(string title, Uri url, int duration)
        {
            Title = title;
            Url = url;
            Duration = duration;
        }
        public string Title { get; set; }
        public Uri Url { get; set; }
        public int Duration { get; set; }
    }
}
