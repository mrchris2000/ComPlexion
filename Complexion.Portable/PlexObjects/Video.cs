using System.IO;

namespace Complexion.Portable.PlexObjects
{
    public class Video
    {
        public string title { get; set; }
        public string summary { get; set; }
        public string guid { get; set; }
        public string art { get; set; }
        public string thumb { get; set; }

        public string ImdbLink
        {
            get { return guid.Replace("com.plexapp.agents.imdb://", "http://www.imdb.com/title/"); }
        }

        public MemoryStream ThumbImage { get; set; }
        public MemoryStream ArtImage { get; set; }
    }
}
