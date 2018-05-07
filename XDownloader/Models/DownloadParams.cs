using System.ComponentModel.DataAnnotations;

namespace XDownloader.Models
{
    /// <summary>
    /// Model for the api/Download route parameters.
    /// </summary>
    public class DownloadParameters
    {
        #region Public Properties

        /// <summary>
        /// Destination to where we have to save the file.
        /// </summary>
        [Required]
        public string DestinationPath { get; set; }

        /// <summary>
        /// Link from which we have to get the file
        /// </summary>
        [Url]
        [Required]
        public string DownloadLink { get; set; }
        /// <summary>
        /// The name of the downloaded file.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        #endregion Public Properties
    }
}