using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;

namespace FindTermInWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.ElementAtOrDefault(0) == null || args.ElementAtOrDefault(1) == null || String.IsNullOrWhiteSpace(args[0]) || String.IsNullOrWhiteSpace(args[1]))
            //{
            //    Console.WriteLine("PARAMETROS INVALIDOS");
            //    return;
            //}

            Uri uri;
            string term;// = args[1];
            bool useChildConstraint = false;//args.ElementAtOrDefault(2) == null || args[2] == "true";

            try
            {
                // uri = ClearUrl(new Uri(args[0]));
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("\nINVALID URL FORMAT");
                return;
            }

            uri = ClearUrl(new Uri(@"http://thiagopessia.com/projetos/teste-find-term-in-website"));
            term = "Termo";


            Console.WriteLine("Website: " + uri.AbsoluteUri);
            Console.WriteLine("Termo: " + term);
            Console.WriteLine("Initiating application...\n");


            var pagesHistory = new List<Uri>();
            var pagesQueue = new Queue<Uri>();
            var pagesFound = new List<Uri>();

            pagesQueue.Enqueue(uri);

            while (pagesQueue.Any())
            {
                // Create HTML doc
                var uriTemp = pagesHistory.Any() ? ResolveUri(pagesHistory[pagesHistory.Count - 1], pagesQueue.Dequeue().AbsoluteUri) : pagesQueue.Dequeue();

                Console.WriteLine("Searching at " + uriTemp.AbsoluteUri);

                HtmlWeb web = new HtmlWeb();
                web.PreRequest = OnPreRequest;
                var htmlDoc = web.Load(uriTemp.AbsoluteUri).DocumentNode;

                // Search for term in page
                var termRegex = new Regex(term);
                if (termRegex.IsMatch(htmlDoc.OuterHtml))
                    pagesFound.Add(uriTemp);


                // Search for links in page
                var links = htmlDoc.QuerySelectorAll("a[href]") // Get all anchors
                    .Select(n => ClearUrl(ResolveUri(uriTemp, n.Attributes["href"].Value))) // Get anchors uri
                    .Distinct() // Remove duplicates
                    .Where(u => u.Host == uriTemp.Host && (!useChildConstraint || IsChildUri(uri, u.AbsoluteUri))) // Select only from same domain and child
                    .ToList();
                links.RemoveAll(u => u.AbsoluteUri == uriTemp.AbsoluteUri || pagesHistory.Any(h => h.AbsoluteUri == u.AbsoluteUri) || pagesQueue.Any(q => q.AbsoluteUri == u.AbsoluteUri)); // Remove already verified

                foreach (var link in links)
                    pagesQueue.Enqueue(link);

                pagesHistory.Add(uriTemp);

            }

            Console.WriteLine("\n\n--------------------");
            Console.WriteLine("COMPLETED\nTerm found at:\n");
            foreach (var uriFound in pagesFound)
            {
                Console.WriteLine(uriFound.AbsoluteUri);
            }
        }

        private static bool OnPreRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = true;
            return true;
        }

        public static Uri ClearUrl(Uri uri) // Remove hash and query string
        {
            var url = uri.AbsoluteUri;
            var indexHash = url.IndexOf('#');
            var indexQuery = url.IndexOf('?');
            var subIndex = indexHash == indexQuery ? url.Length : (indexHash > indexQuery ? indexHash : indexQuery);

            url = url.Substring(0, subIndex);
            var hasExt = new Regex(@"^(www\.|(?:http|ftp)s?:\/\/|[A-Za-z]:\\|\/\/).*(\/.*\..*)$");
            if (!hasExt.IsMatch(url))
                url = url.Trim('/') + "/";

            return new Uri(url);
        }

        public static Uri ResolveUri(Uri currentUri, string linkUrl)
        {
            //var absolute = new Regex(@"^(www\.|(?:http|ftp)s?:\/\/|[A-Za-z]:\\|\/\/).*"); // https://www.thiago.com.br/teste/index.html
            //var relative1 = new Regex(@"^(?!www\.|(?:http|ftp)s?:\/\/|[A-Za-z]:\\|\/\/)(\/).*"); // /teste/index.html
            //var relative2 = new Regex(@"^(?!www\.|(?:http|ftp)s?:\/\/|[A-Za-z]:\\|\/\/)(?!\/|\.\.\/).*"); // index.html
            //var relative3 = new Regex(@"^(?!www\.|(?:http|ftp)s?:\/\/|[A-Za-z]:\\|\/\/)(?!\/|\.\.\/).*"); // ../index.html

            var currentUrl = currentUri.AbsoluteUri;
            var hasExt = new Regex(@"^(www\.|(?:http|ftp)s?:\/\/|[A-Za-z]:\\|\/\/).*(\/.*\..*)$");
            if (!hasExt.IsMatch(currentUrl))
                currentUrl = currentUrl.Trim('/') + "/";

            return new Uri(new Uri(currentUrl), linkUrl);
        }

        public static bool IsChildUri(Uri parentUri, string targetUrl)
        {
            var linkUri = ResolveUri(parentUri, targetUrl);

            var currentPathArr = parentUri.AbsolutePath.Trim('/').Split('/');
            var linkPathArr = linkUri.AbsolutePath.Trim('/').Split('/');

            if (currentPathArr.Length > linkPathArr.Length)
                return false;

            for (int i = 0; i < currentPathArr.Length; i++)
            {
                if (currentPathArr[i] != linkPathArr[i])
                    return false;
            }

            return true;
        }
    }
}
