using HtmlAgilityPack;

namespace InstaParse.Interfaces
{
    public interface IHtmlParser
    {
        HtmlDocument GetDocumentByUrl(string url);

        HtmlDocument GetDocumentFromString(string html);
    }
}