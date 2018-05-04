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

        public ApiController(IOptions<XConfig> xConfig, IConfiguration Configuration)
        {
            this.xConfig = xConfig.Value;
            this.configuration = Configuration;
        }

        #endregion Public Constructors

        #region Public Properties

        public string ChromeDriverDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public ChromeOptions ChromeOptions
        {
            get
            {
                chromeOptions.AddArgument("headless");
                return chromeOptions;
            }
            set => chromeOptions = value;
        }

        public IConfiguration Configuration { get; set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadLink"></param>
        /// <param name="destinationPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpPost("Download", Name = "Execute_Download")]
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
            return Ok($"Le téléchargement du fichier {fileName} est en cours à l'emplacement {destinationPath}.");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost("LinksFromProtector", Name = "Get_Link_From_Protector_Page")]
        public IActionResult GetLinkFromProtector([FromBody] RequestParameters parameters)
        {
            if (parameters != null && !string.IsNullOrEmpty(parameters.Url))
            {
                using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
                {
                    string link = FindHostUnderProtector(parameters.Url, driver);
                    string relativeLink = string.Empty;

                    Host host = xConfig.Hosts.First(h => h.Name == "uptobox");
                    // traitement du cas ou le lien est n'est pas en https
                    if (link.Contains("https"))
                        relativeLink = link.Replace(host.URL, string.Empty);
                    else
                        relativeLink = link.Replace(host.URL.Replace("https", "http"), string.Empty);

                    #region Host Authentication And Redirection

                    // authentication on uptobox.com
                    driver.Navigate().GoToUrl($"{host.URL}?op=login&referer={relativeLink}");
                    driver.FindElementByCssSelector("#login-form > input[name=login]").SendKeys(host.Login);
                    driver.FindElementByCssSelector("#login-form > input[name=password]").SendKeys(host.Password);
                    driver.FindElementByCssSelector("#login-form > div > button").Click();

                    #endregion Host Authentication And Redirection

                    // On récupère le lien de téléchargement fourni par l'hébergeur
                    string downloadLink = driver.FindElementByCssSelector("#dl > form > div > center > a").GetAttribute("href");
                    int slashIndex = downloadLink.LastIndexOf('/');
                    // On supprime le '/' se trouvant au début de l'URL.
                    string fileName = downloadLink.Substring(slashIndex + 1, downloadLink.Length - slashIndex - 1);
                    return ExecuteDownload(downloadLink, Configuration["DestinationDownload"], fileName);
                }
            }
            else
            {
                return BadRequest($"Un ou plusieurs paramètres d'entrés sont manquants voici le contenu de vos paramètres : {parameters.ToString()}");
            }
        }

        [HttpPost("LinksFromSource", Name = "Get_All_Links_From_Source_Page")]
        public IActionResult GetLinksFromSource([FromBody] RequestParameters parameters)
        {
            List<string> protectedLinks = new List<string>();

            using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
            {
                driver.Navigate().GoToUrl(parameters.Url);
                var links = driver.FindElementsByCssSelector("a[href*=\"dl-protect\"]");

                foreach (var link in links)
                {
                    protectedLinks.Add(link.GetAttribute("href"));
                }

                if (parameters.Hosts.Count == 0)
                    return Json(protectedLinks);
                else
                    return Json(FilterUriList(protectedLinks, parameters.Hosts));
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static string FindHostUnderProtector(string protecterURL, ChromeDriver driver)
        {
            try
            {
                driver.Navigate().GoToUrl(protecterURL);
                driver.FindElement(By.ClassName("continuer")).Click();
                var link = driver.FindElementByCssSelector("div.lienet > a[href*=\"http\"]").Text;
                return link;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<string> FilterUriList(List<string> protectedLinks, List<string> desiredHosts)
        {
            using (var driver = new ChromeDriver(ChromeDriverDirectory, ChromeOptions))
            {
                List<string> uriList = new List<string>();
                foreach (string protectedLink in protectedLinks)
                {
                    string hostedLink = FindHostUnderProtector(protectedLink, driver);
                    if (!string.IsNullOrEmpty(hostedLink))
                    {
                        foreach (string desiredHost in desiredHosts)
                        {
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