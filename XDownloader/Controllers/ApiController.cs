using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using XDownloader.Models;

namespace XDownloader.Controllers
{
    /// <summary>
    /// Main class of the application. It provides the main features like : perform a download.
    /// </summary>
    [Produces("application/json")]
    [Route("api")]
    public class ApiController : Controller
    {
        #region Private Fields

        private readonly IConfiguration configuration;
        private readonly XConfig xConfig;
        private ChromeOptions chromeOptions = new ChromeOptions();

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Public constructor of the ApiController. DI used, don't inject yourself the parameters.
        /// </summary>
        /// <param name="xConfig">The specific section XConfig of appsettings.*.json loaded in memory.</param>
        /// <param name="configuration">The entire configuration object loaded from appsettings.*.json</param>
        public ApiController(IOptions<XConfig> xConfig, IConfiguration configuration)
        {
            this.xConfig = xConfig.Value;
            this.configuration = configuration;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Contains the location of the ChromeDriver location on the physical drive.
        /// </summary>
        public string ChromeDriverDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        /// <summary>
        /// Contains the options for the ChromeDriver. It is actually hardcoded with 'headless' option.
        /// </summary>
        public ChromeOptions ChromeOptions
        {
            get
            {
                chromeOptions.AddArgument("headless");
                return chromeOptions;
            }
            set => chromeOptions = value;
        }

        /// <summary>
        /// Access the Configuration.
        /// </summary>
        public IConfiguration Configuration { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Perform a file download from the downloadLink to the destinationPath with the fileName provided.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "downloadLink":"http://www9.uptobox.com/dl/S97L9yxcI1bP3qBxhfyUCK3GWp3iK1X4cHKCRi5y0b5sT5OJqF-mjhUkPnGXK85iqA-aIqDAcfq18gwjMv_LtDI6zAHZkg1GhJcxVOZoVEs5V2Y6GZ5KvCrjxtb1eyJkG_r99QNdKYw3ap_QXkv19Q/Gotham.S04E20.FASTSUB.VOSTFR.720p.WEBRiP.x264-GODSPACE.WwW.Zone-Telechargement1.com.mkv",
        ///         "destinationPath":"/path/show/season/",
        ///         "filename":"Gotham.S04E20.FASTSUB.VOSTFR.720p.WEBRiP.x264-GODSPACE.WwW.Zone-Telechargement1.com.mkv"
        ///     }
        /// </remarks>
        /// <param name="parameters">The <see cref="DownloadParameters"/> model which contains the necessary values.</param>
        /// <returns></returns>
        /// <response code="200">The asynchronous download has started correctly.</response>
        /// <response code="400">
        /// If the <see cref="ArgumentNullException"/> is raised, it is either because the downloadLink or the destinationPath is unset or incorrect.
        /// </response>
        /// <response code="500">
        /// If the <see cref="WebException"/> is raised, it is because a network error occured and the download failed or else it is the <see
        /// cref="InvalidOperationException"/> that has been raised if any error occured.
        /// </response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [HttpPost("Download", Name = "Execute_Download")]
        public IActionResult ExecuteDownload([FromBody] DownloadParameters parameters)
        {
            WebClient wb = new WebClient();
            try
            {
                // Starts the download asynchronously
                wb.DownloadFileAsync(new Uri(parameters.DownloadLink), $"{parameters.DestinationPath + parameters.FileName}");
            }
            catch (ArgumentNullException ane)
            {
                return BadRequest($"L'URL suivante est peut-être incorrecte : {parameters.DownloadLink} ?\n Ou le chemin d'enregistrement : {parameters.DestinationPath} est inconnu ?\n Exception : {ane}");
            }
            catch (WebException we)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, we);
            }
            catch (InvalidOperationException ioe)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ioe);
            }
            return Ok($"Le téléchargement du fichier {parameters.FileName} est en cours à l'emplacement suivant : {parameters.DestinationPath}");
        }

        /// <summary>
        /// Retrieve the link under the protected URL. It only works for the Uptobox host for now.
        /// </summary>
        /// <remarks>
        /// Minimal sample request:
        ///
        ///     {
        ///         "url": "https://www.dl-protect1.com/1234556001234556021234556101234556156bsfkx1psaa8"
        ///     }
        ///
        /// Extended sample request
        ///
        ///     {
        ///         "url": "https://www.dl-protect1.com/1234556001234556021234556101234556156bsfkx1psaa8",
        ///         "hosts": [ "uptobox", "1fichier" ] 
        ///     }
        /// </remarks>
        /// <param name="parameters">The request parameters serialize as an object.</param>
        /// <returns>The host link found from the protected link given through the <paramref name="parameters"/></returns>
        /// <response code="200">
        /// A filtered list of url unprotected if a host was provided.
        /// Otherwise it will return a list of all the protected url found on the source page.
        /// </response>
        /// <response code="400">
        /// If a required parameter was missing this code is return.
        /// Yous should check the reponse message to understand what was missing.
        /// </response>
        /// <response code="404">
        /// If no host link could be found from the protected link.
        /// </response>
        [HttpPost("LinksFromProtector", Name = "Get_Link_From_Protector_Page")]
        public IActionResult GetLinkFromProtector([FromBody] RequestParameters parameters)
        {
            // Check if the necessary properties are defined.
            if (parameters != null && !string.IsNullOrEmpty(parameters.Url))
            {
                using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
                {
                    string link = FindHostLinkUnderProtector(parameters.Url, driver);
                    if (string.IsNullOrEmpty(link))
                        return NotFound($"Impossible de trouver un lien d'hébergeur pour les paramètres suivants : {parameters.ToString()}");

                    string relativeLink = string.Empty;

                    Host host = xConfig.Hosts.First(h => h.Name == "uptobox");
                    // Handle the case where the link doesn't start with "https"
                    if (link.Contains("https"))
                        relativeLink = link.Replace(host.URL, string.Empty);
                    else
                        relativeLink = link.Replace(host.URL.Replace("https", "http"), string.Empty);

                    #region Host Authentication And Redirection

                    // Navigate to the login page of uptobox.com and define the redirection to the former link.
                    driver.Navigate().GoToUrl($"{host.URL}?op=login&referer={relativeLink}");
                    // Put the login defined in configuration
                    driver.FindElementByCssSelector("#login-form > input[name=login]").SendKeys(host.Login);
                    // Put the password defined in configuration
                    driver.FindElementByCssSelector("#login-form > input[name=password]").SendKeys(host.Password);
                    // Perform the click to finally authenticate
                    driver.FindElementByCssSelector("#login-form > div > button").Click();

                    #endregion Host Authentication And Redirection

                    // Retrieve the link provided by the host under the download button
                    string downloadLink = driver.FindElementByCssSelector("#dl > form > div > center > a").GetAttribute("href");
                    return Ok(new { Link = downloadLink, FileName = ExtractFilenameFromDownloadLink(downloadLink) });
                }
            }
            else
            {
                return BadRequest($"Un ou plusieurs paramètres d'entrés sont manquants voici le contenu de vos paramètres : {parameters.ToString()}");
            }
        }

        /// <summary>
        /// Return a list of links scraped from the source URL given in the <paramref name="parameters"/> under the property URL.
        /// </summary>
        /// <remarks>
        /// Minimal sample request:
        ///
        ///     { 
        ///         "url": "https://ww2.zone-telechargement1.com/34408-jumanji-bienvenue-dans-la-jungle-french-bdrip.html" 
        ///     }
        ///
        /// Extended sample request
        ///
        ///     { 
        ///         "url": "https://ww2.zone-telechargement1.com/34408-jumanji-bienvenue-dans-la-jungle-french-bdrip.html",
        ///         "hosts": [ "uptobox", "1fichier" ] 
        ///     }
        /// </remarks>
        /// <param name="parameters">The request parameters serialize as an object.</param>
        /// <returns>
        /// If there is at least one host define in the Hosts property of <paramref name="parameters"/> then it will return a filtered list of url now
        /// unprotected as a JSON object. Otherwise it will return a list of all the protected url found on the source page.
        /// </returns>
        /// <response code="200">
        /// A filtered list of url unprotected if a host was provided.
        /// Otherwise it will return a list of all the protected url found on the source page.
        /// </response>
        /// <response code="400">
        /// If a required parameter was missing this code is return.
        /// Yous should check the reponse message to understand what was missing.
        /// </response>
        /// <response code="404">
        /// If no protected links were found on the given page then it will return an empty response.
        /// </response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPost("LinksFromSource", Name = "Get_All_Links_From_Source_Page")]
        public IActionResult GetLinksFromSource([FromBody] RequestParameters parameters)
        {
            // Check if the necessary properties are defined.
            if (parameters != null && !string.IsNullOrEmpty(parameters.Url))
            {
                using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
                {
                    List<string> protectedLinks = new List<string>();
                    // Navigate to the given URL.
                    driver.Navigate().GoToUrl(parameters.Url);
                    // Get all html element that contains "dl-protect" in their href attribute.
                    var links = driver.FindElementsByCssSelector("a[href*=\"dl-protect\"]");

                    // If no links were found
                    if (links.Count == 0)
                        return NotFound($"Il n'y avait aucun lien protégés sur la page chargée à partir de l'url {parameters.Url}");

                    // For each element retrieve the actual URL
                    foreach (var link in links)
                    {
                        protectedLinks.Add(link.GetAttribute("href"));
                    }

                    // If there are no host then return the list of protected URL.
                    if (parameters.Hosts == null || parameters.Hosts.Count == 0)
                        return Ok(protectedLinks);
                    // Else we filter the list of protected URL by hosts list.
                    else
                        return Ok(FilterUriList(protectedLinks, parameters.Hosts));
                } 
            }
            else
            {
                return BadRequest($"Un ou plusieurs paramètres d'entrés sont manquants voici le contenu de vos paramètres : {parameters.ToString()}");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Extract the filename that will be downlaoded from the <paramref name="downloadLink"/>.
        /// </summary>
        /// <param name="downloadLink">Url from which it will extract the filename.</param>
        /// <returns>The extracted filename of the given url.</returns>
        private static string ExtractFilenameFromDownloadLink(string downloadLink)
        {
            // The filename is always after the last slash of the url.
            int slashIndex = downloadLink.LastIndexOf('/');
            // Delete the started '/' from the URL.
            return downloadLink.Substring(slashIndex + 1, downloadLink.Length - slashIndex - 1);
        }

        /// <summary>
        /// Retrieve the host link under the <paramref name="protecterURL"/>.
        /// </summary>
        /// <param name="protecterURL">The protected URL from which we want to find the host link.</param>
        /// <param name="driver"></param>
        /// <returns>The host link unprotected.</returns>
        private static string FindHostLinkUnderProtector(string protecterURL, ChromeDriver driver)
        {
            try
            {
                // Navigate to the given url
                driver.Navigate().GoToUrl(protecterURL);
                // Find the button and perform a click and wait for the implicit refresh which happen.
                driver.FindElement(By.ClassName("continuer")).Click();
                // Retrieve the link by css selector, this will stop working at the moment where dl-protect will change their html structure.
                return driver.FindElementByCssSelector("div.lienet > a[href*=\"http\"]").Text;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Filter the list of protected url by the list of given host we want to keep.
        /// </summary>
        /// <param name="protectedLinks">The list of protected url to filter.</param>
        /// <param name="desiredHosts">The list of host to power the filter.</param>
        /// <returns>The list of unprotected url filtered by the <paramref name="desiredHosts"/>.</returns>
        private List<string> FilterUriList(List<string> protectedLinks, List<string> desiredHosts)
        {
            using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
            {
                List<string> uriList = new List<string>();
                // For each protected url
                foreach (string protectedLink in protectedLinks)
                {
                    // Find the actual host link
                    string hostedLink = FindHostLinkUnderProtector(protectedLink, driver);
                    if (!string.IsNullOrEmpty(hostedLink))
                    {
                        foreach (string desiredHost in desiredHosts)
                        {
                            // If the link contains the dns of an host then we add it to the final list.
                            if (hostedLink.Contains(desiredHost))
                                uriList.Add(hostedLink);
                        }
                    }
                }
                return uriList;
            }
        }

        #endregion Private Methods
    }
}