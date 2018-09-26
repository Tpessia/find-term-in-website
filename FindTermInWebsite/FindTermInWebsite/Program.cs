using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FindTermInWebsite.Helpers;
using HtmlAgilityPack;
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
            bool useChildConstraint;//args.ElementAtOrDefault(2) == null || args[2] == "true";

            try
            {
                // uri = ClearUrl(new Uri(args[0])).ClearUrl();
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("\nINVALID URL FORMAT");
                return;
            }

            uri = new Uri(@"https://www.nestleprofessional.com.br").ClearUrl();
            term = "Noblesse";
            useChildConstraint = true;

            Console.WriteLine("Website: " + uri.AbsoluteUri);
            Console.WriteLine("Termo: " + term);
            Console.WriteLine("Initiating search...\n");

            var crawler = new WebCrawler(uri);
            var pagesFound = crawler.Search(term, useChildConstraint);


            Console.WriteLine("\nBUSCA FINALIZADA:");
            Console.WriteLine($"Termo encontrado {pagesFound.Count} vezes.\n");
            foreach (var uriFound in pagesFound)
            {
                Console.WriteLine(uriFound.AbsoluteUri);
            }
        }
    }
}
