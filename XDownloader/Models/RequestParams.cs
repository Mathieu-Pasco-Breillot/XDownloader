using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XDownloader.Models
{
    /// <summary>
    /// Model which represent the content of any request parameters send.
    /// </summary>
    public class RequestParameters
    {
        #region Public Properties

        /// <summary>
        /// The list of hosts that should be handle by the scrapper.
        /// </summary>
        public List<string> Hosts { get; set; }

        /// <summary>
        /// Any kind of url, it may be a protected url or a source url.
        /// </summary>
        [Url]
        [Required]
        public string Url { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Display the properties content.
        /// </summary>
        /// <returns>A string which contains the given parameters content.</returns>
        public override string ToString()
        {
            string hosts = string.Empty;
            if (Hosts != null && Hosts.Count != 0)
                Hosts.ForEach(x => hosts += $"{x},");
            return $"URL : {Url} | Hosts : {hosts}";
        }

        #endregion Public Methods
    }
}