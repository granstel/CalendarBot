using HtmlAgilityPack;

namespace CalendarBot.Services
{
    public interface IHtmlParser
    {
        HtmlDocument GetDocumentByUrl(string url);

        HtmlDocument GetDocumentFromString(string html);
    }
}