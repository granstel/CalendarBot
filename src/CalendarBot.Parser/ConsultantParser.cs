using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using InstaParse.Interfaces;
using NLog;

namespace InstaParse.Parsers
{
    public class ConsultantParser
    {
        private readonly IHtmlParser _htmlParser;

        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly string _url = ConfigurationManager.AppSettings["consultantUrl"];


        public ConsultantParser()
        {
            _htmlParser = new HtmlParser();
        }

        public void ParseCalendar()
        {
            var document = _htmlParser.GetDocumentByUrl(_url);

            var calendars = document.DocumentNode.SelectNodes("//*[@class=\"cal\"]");

            var monthDictionary = new Dictionary<string, Month>();

            foreach (var calendar in calendars)
            {
                var monthNode = calendar.SelectNodes($"{calendar.XPath}//th[@class=\"month\"]");

                var monthName = monthNode.Select(m => m.InnerText).FirstOrDefault();

                var month = new Month(monthName);

                if (monthName != null && !monthDictionary.ContainsKey(monthName))
                {
                    monthDictionary.Add(monthName, month);
                }

                var holidaysNodes = calendar.SelectNodes($"{calendar.XPath}//td[@class=\"holiday weekend\"]");

                var holidays = holidaysNodes?.Select(n => n.InnerText).ToList();

                if(holidays != null)
                    month.Holidays.AddRange(holidays);

                var preHolidaysNodes = calendar.SelectNodes($"{calendar.XPath}//td[@class=\"preholiday\"]");

                var preHolidays = preHolidaysNodes?.Select(n => n.InnerText).ToList();

                if(preHolidays != null)
                    month.PreHolidays.AddRange(preHolidays);

                var weekNodes = calendar.SelectNodes($"{calendar.XPath}//td[@class=\"weekend\"]");

                var weekends = weekNodes.Select(n => n.InnerText).ToList();

                if(weekNodes != null)
                    month.Weekends.AddRange(weekends);
            }
        }
    }
}
