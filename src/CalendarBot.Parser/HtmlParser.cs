using HtmlAgilityPack;
using InstaParse.Interfaces;

namespace InstaParse.Parsers
{
    public class HtmlParser : IHtmlParser
    {
        public HtmlDocument GetDocumentByUrl(string url)
        {
            var client = new HtmlWeb();

            var document = client.Load(url);

            return document;
        }

        public HtmlDocument GetDocumentFromString(string html)
        {
            var document = new HtmlDocument();

            document.LoadHtml(html);

            return document;
        }
    }
}
