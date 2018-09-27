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
            if (args.ElementAtOrDefault(0) == null || args.ElementAtOrDefault(1) == null || String.IsNullOrWhiteSpace(args[0]) || String.IsNullOrWhiteSpace(args[1]))
            {
                Console.WriteLine("PARAMETROS INVALIDOS");
                return;
            }

            Uri uri;
            string term = args[1];
            bool useChildConstraint = args.ElementAtOrDefault(2) == null || args[2] == "true";

            try
            {
                uri = new Uri(args[0]).ClearUrl();
            }
            catch (UriFormatException e)
            {
                Console.WriteLine("\nINVALID URL FORMAT");
                return;
            }

            Console.WriteLine("Website: " + uri.AbsoluteUri);
            Console.WriteLine("Termo: " + term);
            Console.WriteLine("Iniciando pesquisa...\n");

            var crawler = new WebCrawler(uri);
            var pagesFound = crawler.Search(term, useChildConstraint);


            Console.WriteLine("\nBUSCA FINALIZADA:");
            Console.WriteLine($"Termo encontrado {pagesFound.Count} vezes.\n");
            foreach (var uriFound in pagesFound)
            {
                Console.WriteLine(uriFound.AbsoluteUri);
            }

            var filePath = @"C:\Temp\FindTermInWebsite_Results\pesquisa_" + DateTime.Now.ToString("s").Replace(":", "-") + ".txt";
            var directoryInfo = (new System.IO.FileInfo(filePath)).Directory;
            directoryInfo?.Create();
            System.IO.File.WriteAllText(filePath, $"Website: {uri.OriginalString}\r\nTermo: {term}\r\n\r\nPáginas encontradas:\r\n" + String.Join("\r\n", pagesFound));
        }
    }
}
