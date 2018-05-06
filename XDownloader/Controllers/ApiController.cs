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
        /// Perform a file download from the <paramref name="downloadLink"/> to the <paramref name="destinationPath"/> with the <paramref name="fileName"/>.
        /// </summary>
        /// <param name="downloadLink">Link from which we have to get the file.</param>
        /// <param name="destinationPath">Destination to where we have to save the file.</param>
        /// <param name="fileName">The name of the downloaded file.</param>
        /// <returns>
        /// HTTP 200 if the asynchronous download has started correctly. HTTP 400 if either the <paramref name="downloadLink"/> or the <paramref
        /// name="destinationPath"/> is incorrect. HTTP 500 if a network error occured and the download failed. HTTP 500 if a generic error occured. Please refer
        /// to the exception.
        /// </returns>
        /// <exception cref="ArgumentNullException">Raised if either the <paramref name="downloadLink"/> or the <paramref name="destinationPath"/> is incorrect.</exception>
        /// <exception cref="WebException">Raised if a network error occured and the download failed.</exception>
        /// <exception cref="InvalidOperationException">Raised if any error occurs.</exception>
        public IActionResult ExecuteDownload(string downloadLink, string destinationPath, string fileName)
        {
            WebClient wb = new WebClient();
            try
            {
                // On lance le téléchargement du fichier de façon asynchrone.
                wb.DownloadFileAsync(new Uri(downloadLink), $"{destinationPath + fileName}");
            }
            catch (ArgumentNullException ane)
            {
                return BadRequest($"l'URL suivante est peut-être incorrecte : {downloadLink} ?\n Ou le chemin d'enregistrement : {destinationPath} est inconnu ?\n Exception : {ane}");
            }
            catch (WebException we)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, we);
            }
            catch (InvalidOperationException ioe)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ioe);
            }
            return Ok($"Le téléchargement du fichier {fileName} est en cours à l'emplacement suivant : {destinationPath}");
        }

        /// <summary>
        /// Retrieve the link under the protected URL. It only works for the Uptobox host.
        /// </summary>
        /// <param name="parameters">The request parameters serialize as an object.</param>
        /// <returns>HTTP 400 if one of the URL property is missing from the <paramref name="parameters"/> object.</returns>
        [HttpPost("LinksFromProtector", Name = "Get_Link_From_Protector_Page")]
        public IActionResult GetLinkFromProtector([FromBody] RequestParameters parameters)
        {
            // Check if the necessary properties are defined.
            if (parameters != null && !string.IsNullOrEmpty(parameters.Url))
            {
                using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
                {
                    string link = FindHostUnderProtector(parameters.Url, driver);
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
                    int slashIndex = downloadLink.LastIndexOf('/');
                    // Delete the started '/' from the URL.
                    string fileName = downloadLink.Substring(slashIndex + 1, downloadLink.Length - slashIndex - 1);
                    return ExecuteDownload(downloadLink, Configuration["DestinationDownload"], fileName);
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
        /// <param name="parameters">The request parameters serialize as an object.</param>
        /// <returns>
        /// If there is at least one host define in the Hosts property of <paramref name="parameters"/> then it will return a filtered list of url now
        /// unprotected as a JSON object. Otherwise it will return a list of all the protected url found on the source page.
        /// </returns>
        [HttpPost("LinksFromSource", Name = "Get_All_Links_From_Source_Page")]
        public IActionResult GetLinksFromSource([FromBody] RequestParameters parameters)
        {
            using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
            {
                List<string> protectedLinks = new List<string>();
                // Navigate to the given URL.
                driver.Navigate().GoToUrl(parameters.Url);
                // Get all html element that contains "dl-protect" in their href attribute.
                var links = driver.FindElementsByCssSelector("a[href*=\"dl-protect\"]");

                // For each element retrieve the actual URL
                foreach (var link in links)
                {
                    protectedLinks.Add(link.GetAttribute("href"));
                }

                // If there are no host then return the list of protected URL.
                if (parameters.Hosts.Count == 0)
                    return Json(protectedLinks);
                // Else we filter the list of protected URL by hosts list.
                else
                    return Json(FilterUriList(protectedLinks, parameters.Hosts));
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Retrieve the host link under the <paramref name="protecterURL"/>.
        /// </summary>
        /// <param name="protecterURL">The protected URL from which we want to find the host link.</param>
        /// <param name="driver"></param>
        /// <returns>The host link unprotected.</returns>
        private static string FindHostUnderProtector(string protecterURL, ChromeDriver driver)
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
                    string hostedLink = FindHostUnderProtector(protectedLink, driver);
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