using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        [HttpPost("LinksFromHoster", Name = "Get_All_Links_From_Page")]
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