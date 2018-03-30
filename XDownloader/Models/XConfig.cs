using System.Collections.Generic;

namespace XDownloader.Models
{
    public class XConfig
    {
        public List<Source> Sources { get; set; }
    }

    public class Source
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public List<Show> Shows { get; set; }
    }

    public class Show
    {
        public string Name { get; set; }
        public List<Season> Seasons { get; set; }
    }

    public class Season
    {
        public int Number { get; set; }
        public List<Link> Links { get; set; }
    }

    public class Link
    {
        public string Source { get; set; }
        public string Destination { get; set; }
    }
}