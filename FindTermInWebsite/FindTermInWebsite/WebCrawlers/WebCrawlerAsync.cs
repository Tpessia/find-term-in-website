using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindTermInWebsite.Extensions;
using HtmlAgilityPack;

namespace FindTermInWebsite.WebCrawlers
{
    class WebCrawlerAsync : WebCrawler
    {
        public Task<HtmlDocument> NextHtmlDoc { get; set; }

        public WebCrawlerAsync(Uri initialUri) : base(initialUri)
        {
        }

        protected override HtmlNode LoadHtmlNode(Uri uri)
        {
            var web = new HtmlWeb { PreRequest = OnPreRequest };

            if (NextHtmlDoc != null)
            {
                var htmlNode = NextHtmlDoc.Result.DocumentNode;

                if (PagesQueue.Any())
                {
                    var nextUri = PagesQueue.Peek();
                    NextHtmlDoc = web.LoadFromWebAsync(nextUri.AbsoluteUri);
                }

                return htmlNode;
            }
            else
            {
                if (PagesQueue.Any())
                {
                    var nextUri = PagesQueue.Peek();
                    NextHtmlDoc = web.LoadFromWebAsync(nextUri.AbsoluteUri);
                }

                return web.LoadFromWebAsync(uri.AbsoluteUri).Result.DocumentNode;
            }
        }
    }
}
