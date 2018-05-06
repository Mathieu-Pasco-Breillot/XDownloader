using System.Collections.Generic;

namespace XDownloader.Models
{
    /// <summary>
    /// Host from which we will try to get a link and perform a download.
    /// </summary>
    /// <remarks>If you have a paid account it will perform the download as premium user.</remarks>
    public class Host
    {
        #region Public Properties

        /// <summary>
        /// Host username that will allow us to authenticate yourself.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Host name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Host password that will allow us to authenticate yourself.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Host main domain
        /// </summary>
        public string URL { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Link from which we will retrieve the final link to download.
    /// </summary>
    public class Link
    {
        #region Public Properties

        /// <summary>
        /// Relative destination path Example : /home/mathieu/downloads/tvshows/MyShow/SeasonN/
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Source link
        /// </summary>
        public string Source { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Season we want to watch.
    /// </summary>
    public class Season
    {
        #region Public Properties

        /// <summary>
        /// List of all source link from which to download.
        /// </summary>
        /// <remarks>You can put here multiple links, one for the 720p version and another one for the 1080p version.</remarks>
        public List<Link> Links { get; set; }

        /// <summary>
        /// Season number
        /// </summary>
        public int Number { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Show we want to watch.
    /// </summary>
    public class Show
    {
        #region Public Properties

        /// <summary>
        /// Show name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of all seasons we want to watch.
        /// </summary>
        public List<Season> Seasons { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Source of tvshows where we are going to look for.
    /// </summary>
    public class Source
    {
        #region Public Properties

        /// <summary>
        /// Source name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of all shows we want to look for.
        /// </summary>
        public List<Show> Shows { get; set; }

        /// <summary>
        /// Source main domain
        /// </summary>
        public string URL { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Model to serialize the appsettings.json XConfig section.
    /// </summary>
    public class XConfig
    {
        #region Public Properties

        /// <summary>
        /// List of hosts from which to perform download.
        /// </summary>
        public List<Host> Hosts { get; set; }

        /// <summary>
        /// List of all sources where to look for tvshows.
        /// </summary>
        public List<Source> Sources { get; set; }

        #endregion Public Properties
    }
}