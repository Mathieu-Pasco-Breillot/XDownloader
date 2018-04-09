using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using XDownloader.Models;

namespace XDownloader.Controllers
{
    public class HomeController : Controller
    {
        #region Private Fields

        private readonly XConfig xConfig;
        private static readonly HttpClient client = new HttpClient();

        #endregion Private Fields

        #region Public Constructors

        public HomeController(IOptions<XConfig> xConfig)
        {
            this.xConfig = xConfig.Value;
        }

        #endregion Public Constructors

        #region Public Methods

        [HttpPost]
        public void GetLinksFromHoster([FromBody] string hosterURL)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(hosterURL);

            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");

            foreach (HtmlNode link in links)
            {
                var test = link;
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Index()
        {
            return View();
        }

        #endregion Public Methods
    }
}