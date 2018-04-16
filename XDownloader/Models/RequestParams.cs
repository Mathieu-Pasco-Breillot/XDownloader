using System.Collections.Generic;

namespace XDownloader.Models
{
    public class RequestParameters
    {
        #region Public Properties

        public List<string> Hosts { get; set; }
        public string Url { get; set; }

        #endregion Public Properties

        public override string ToString()
        {
            string hosts = string.Empty;
            Hosts.ForEach(x => hosts += $"{x},");
            return $"URL : {Url} | Hosts : {hosts}";
        }
    }
}