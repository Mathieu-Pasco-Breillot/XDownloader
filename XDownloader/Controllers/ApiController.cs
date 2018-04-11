using HtmlAgilityPack;
using log4net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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

        private readonly XConfig xConfig;

        #endregion Private Fields

        #region Public Constructors

        public ApiController(IOptions<XConfig> xConfig)
        {
            this.xConfig = xConfig.Value;
        }

        #endregion Public Constructors

        #region Public Methods

        [HttpPost("LinksFromProtector", Name = "Get_Link_From_Protector_Page")]
        public IActionResult GetLinkFromProtector([FromBody] string protecterURL)
        {
            var options = new ChromeOptions();
            options.AddArgument("headless");

            using (var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options))
            {
                driver.Navigate().GoToUrl(protecterURL);
                var button = driver.FindElement(By.ClassName("continuer"));
                button.Click();
                var link = driver.FindElement(By.PartialLinkText("uptobox.com")).Text;

                string relativeLink = string.Empty;
                Host host = xConfig.Hosts.First(h => h.Name == "uptobox");

                // traitement du cas ou le lien est n'est pas en https
                if (link.Contains("https"))
                    relativeLink = link.Replace(host.URL, string.Empty);
                else
                    relativeLink = link.Replace(host.URL.Replace("https", "http"), string.Empty);
                // authentication on uptobox.com
                driver.Navigate().GoToUrl($"{host.URL}?op=login&referer={relativeLink}");
                driver.FindElementByCssSelector("#login-form > input[name=login]").SendKeys(host.Login);
                driver.FindElementByCssSelector("#login-form > input[name=password]").SendKeys(host.Password);
                driver.FindElementByCssSelector("#login-form > div > button").Click();

                //driver.Navigate().GoToUrl(link);
                button = driver.FindElementByCssSelector("#dl > form > div > center > a");
                string downloadLink = button.GetAttribute("href");                
                int slashIndex = downloadLink.LastIndexOf('/');
                string fileName = downloadLink.Substring(slashIndex + 1, downloadLink.Length - slashIndex - 1);

                WebClient wb = new WebClient();                

                try
                {
                    wb.DownloadFileAsync(new Uri(downloadLink), $"D:\\Videos\\{fileName}");
                }
                catch (ArgumentNullException ane)
                {
                    return BadRequest($"l'URL suivante est peut-être incorrecte : {downloadLink} ?\n Ou le chemin d'enregistrement : {fileName} est inconnu ?\n Exception : {ane}"); 
                }
                catch (WebException we)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, we);
                }
                catch (InvalidOperationException ioe)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, ioe);
                }
                return Ok($"Le téléchargement du fichier {fileName} est en cours.");
            }
        }

        [HttpPost("LinksFromSource", Name = "Get_All_Links_From_Source_Page")]
        public IActionResult GetLinksFromSource([FromBody] string hosterURL)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(hosterURL);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                List<HtmlAttribute> attributesLinks = new List<HtmlAttribute>();
                foreach (HtmlNode link in links)
                {
                    HtmlAttribute absoluteLink = link.Attributes.First(a => a.Name == "href" && a.Value.StartsWith("https://www.dl-protect"));
                    attributesLinks.Add(absoluteLink);
                }
                if (attributesLinks.Count > 0)
                    return Ok();
                else
                    return NotFound();
            }
            else
            {
                return Unauthorized();
            }
        }

        #endregion Public Methods
    }
}