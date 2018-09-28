using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using FindTermInWebsite.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace FindTermInWebsite.WebCrawlers
{
    class WebCrawler
    {
        public Uri InitialUri { get; set; }
        public List<Uri> PagesHistory { get; set; }
        public Queue<Uri> PagesQueue { get; set; }
        public List<Uri> PagesFound { get; set; }

        public WebCrawler(Uri initialUri)
        {
            InitialUri = initialUri;
        }

        public List<Uri> Search(string term, bool useChildConstraint)
        {
            PagesHistory = new List<Uri>();
            PagesQueue = new Queue<Uri>();
            PagesFound = new List<Uri>();

            PagesQueue.Enqueue(InitialUri);

            while (PagesQueue.Any())
            {
                try
                {
                    // Create HTML doc
                    var uriTemp = GetTempUri();
                    PagesHistory.Add(uriTemp);
                    Console.WriteLine(uriTemp.AbsoluteUri);
                    var htmlNode = LoadHtmlNode(uriTemp);

                    // Search for term in page
                    if (SearchForTerm(term, htmlNode))
                        PagesFound.Add(uriTemp);

                    // Search for links in page
                    var links = GetInternalLink(uriTemp, htmlNode, useChildConstraint);

                    foreach (var link in links)
                        PagesQueue.Enqueue(link);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("INVALID FILE TYPE");
                }
            }

            return PagesFound;
        }

        protected static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = true;
            return true;
        }

        protected Uri GetTempUri()
        {
            return PagesHistory.Any() ? PagesHistory[PagesHistory.Count - 1].ResolveUri(PagesQueue.Dequeue().AbsoluteUri) : PagesQueue.Dequeue();
        }

        protected virtual HtmlNode LoadHtmlNode(Uri uri)
        {
            HtmlWeb web = new HtmlWeb { PreRequest = OnPreRequest };
            return web.Load(uri.AbsoluteUri).DocumentNode;
        }

        private static bool SearchForTerm(string term, HtmlNode htmlNode)
        {
            var termRegex = new Regex(term);
            return termRegex.IsMatch(htmlNode.OuterHtml);
        }

        private List<Uri> GetInternalLink(Uri uri, HtmlNode htmlNode, bool useChildConstraint)
        {
            var links = htmlNode.QuerySelectorAll("a[href]") // Get all anchors
                .Select(n => uri.ResolveUri(n.Attributes["href"].Value).ClearUrl()) // Get anchors uri
                .Distinct() // Remove duplicates
                .Where(u => u.Host == uri.Host && (!useChildConstraint || InitialUri.IsChildUri(u.AbsoluteUri))) // Select only from same domain and child
                .ToList();

            links.RemoveAll(u => PagesHistory.Any(h => h.AbsoluteUri == u.AbsoluteUri) || PagesQueue.Any(q => q.AbsoluteUri == u.AbsoluteUri)); // Remove already verified

            return links;
        }
    }
}
