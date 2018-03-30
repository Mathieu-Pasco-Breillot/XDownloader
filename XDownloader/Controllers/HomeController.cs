using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using XDownloader.Models;

namespace XDownloader.Controllers
{
    public class HomeController : Controller
    {
        #region Private Fields

        private readonly XConfig xConfig;

        #endregion Private Fields

        #region Public Constructors

        public HomeController(IOptions<XConfig> xConfig)
        {
            this.xConfig = xConfig.Value;
        }

        #endregion Public Constructors

        #region Public Methods

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