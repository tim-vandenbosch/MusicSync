using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSync.objects
{
    public class Playlist
    {
        public List<Video> List { get;  }

        public Playlist(List<Video> list)
        {
            this.List = list;
        }
    }
}
