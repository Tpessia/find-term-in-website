using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FindTermInWebsite.Helpers
{
    public static class UriExtensions
    {
        public static Uri ClearUrl(this Uri uri) // Remove hash and query string
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

        public static Uri ResolveUri(this Uri currentUri, string linkUrl)
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

        public static bool IsChildUri(this Uri parentUri, string targetUrl)
        {
            var linkUri = ResolveUri(parentUri, targetUrl);

            var currentPathArr = parentUri.AbsolutePath.Trim('/').Split('/');
            var linkPathArr = linkUri.AbsolutePath.Trim('/').Split('/');

            if (currentPathArr.Length > linkPathArr.Length)
                return false;

            if (String.IsNullOrWhiteSpace(currentPathArr[0]))
                return true;

            for (int i = 0; i < currentPathArr.Length; i++)
            {
                if (currentPathArr[i] != linkPathArr[i])
                    return false;
            }

            return true;
        }
    }
}
