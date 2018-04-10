using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
        public void GetLinkFromProtector([FromBody] string protecterURL)
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
                button = driver.FindElement(By.PartialLinkText("lancer votre téléchargement"));
                string downloadLink = button.GetAttribute("href");
            }
        }

        [HttpPost("LinksFromHoster", Name = "Get_All_Links_From_Hoster_Page")]
        public HttpStatusCode GetLinksFromHoster([FromBody] string hosterURL)
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
                    return HttpStatusCode.OK;
                else
                    return HttpStatusCode.NotFound;
            }
            else
            {
                return HttpStatusCode.Unauthorized;
            }
        }
        #endregion Public Methods
    }
}