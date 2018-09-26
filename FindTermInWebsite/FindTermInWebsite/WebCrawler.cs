using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FindTermInWebsite.Helpers;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace FindTermInWebsite
{
    class WebCrawler
    {
        public Uri InitialUri { get; set; }

        public WebCrawler(Uri initialUri)
        {
            InitialUri = initialUri;
        }

        public List<Uri> Search(string term, bool useChildConstraint)
        {
            var pagesHistory = new List<Uri>();
            var pagesQueue = new Queue<Uri>();
            var pagesFound = new List<Uri>();

            pagesQueue.Enqueue(InitialUri);

            while (pagesQueue.Any())
            {
                // Create HTML doc
                var uriTemp = pagesHistory.Any() ? pagesHistory[pagesHistory.Count - 1].ResolveUri(pagesQueue.Dequeue().AbsoluteUri) : pagesQueue.Dequeue();

                Console.WriteLine("Searching at " + uriTemp.AbsoluteUri);

                HtmlWeb web = new HtmlWeb {PreRequest = OnPreRequest};
                var htmlDoc = web.Load(uriTemp.AbsoluteUri).DocumentNode;

                try
                {
                    // Search for term in page
                    var termRegex = new Regex(term);
                    if (termRegex.IsMatch(htmlDoc.OuterHtml))
                        pagesFound.Add(uriTemp);


                    // Search for links in page
                    var links = htmlDoc.QuerySelectorAll("a[href]") // Get all anchors
                        .Select(n => uriTemp.ResolveUri(n.Attributes["href"].Value).ClearUrl()) // Get anchors uri
                        .Distinct() // Remove duplicates
                        .Where(u => u.Host == uriTemp.Host && (!useChildConstraint || InitialUri.IsChildUri(u.AbsoluteUri))) // Select only from same domain and child
                        .ToList();
                    links.RemoveAll(u => u.AbsoluteUri == uriTemp.AbsoluteUri || pagesHistory.Any(h => h.AbsoluteUri == u.AbsoluteUri) || pagesQueue.Any(q => q.AbsoluteUri == u.AbsoluteUri)); // Remove already verified

                    foreach (var link in links)
                        pagesQueue.Enqueue(link);

                    pagesHistory.Add(uriTemp);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("INVALID FILE TYPE");
                }
            }

            return pagesFound;
        }

        private static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = true;
            return true;
        }
    }
}
