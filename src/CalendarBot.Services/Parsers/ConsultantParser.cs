using System;
using System.Collections.Generic;
using System.Linq;
using CalendarBot.Models.Internal;
using GranSteL.Helpers.Redis;
using NLog;

namespace CalendarBot.Services.Parsers
{
    public class ConsultantParser : IConsultantParser
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IHtmlParser _htmlParser;
        private readonly IRedisCacheService _cache;
        private readonly string _calendarSourceFormat;

        public ConsultantParser(IHtmlParser htmlParser, IRedisCacheService cache, string calendarSourceFormat)
        {
            _htmlParser = htmlParser;
            _cache = cache;
            _calendarSourceFormat = calendarSourceFormat;
        }

        public ICollection<Month> ParseCalendar(string year)
        {
            var _url = string.Format(_calendarSourceFormat, year);

            var document = _htmlParser.GetDocumentByUrl(_url);

            var calendars = document.DocumentNode.SelectNodes("//*[@class=\"cal\"]");

            var monthList = new List<Month>();

            for (var i = 0; i < calendars.Count(); i++)
            {
                var calendar = calendars[i];

                var monthNode = calendar.SelectNodes($"{calendar.XPath}//th[@class=\"month\"]");

                var monthName = monthNode.Select(m => m.InnerText).FirstOrDefault();

                var monthNumber = i + 1;

                var month = new Month(monthName, monthNumber);

                monthList.Add(month);

                var days = calendar.SelectNodes($"{calendar.XPath}//td");

                foreach (var day in days)
                {
                    if (!int.TryParse(day.InnerText, out var number))
                    {
                        _log.Warn($"Can't parse {day.InnerText} from {day.XPath}, {nameof(monthName)}={monthName}");

                        continue;
                    }

                    if (day.Attributes.Any(a => a.Name == "class" && a.Value == "inactively"))
                    {
                        continue;
                    }

                    var monthDay = new Day(number);

                    month.Days.Add(monthDay);

                    if (day.Attributes.Any(a => a.Name == "class" && a.Value == "preholiday"))
                    {
                        monthDay.Type = DayType.PreHoliday;

                        continue;
                    }

                    if (day.Attributes.Any(a => a.Name == "class" && a.Value.Contains("weekend")))
                    {
                        monthDay.Type = DayType.NotWork;

                        continue;
                    }
                }
            }

            try
            {
                _cache.Add($"Calendar:{year}", monthList);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Can't add calendar for {year}");
            }

            return monthList;
        }
    }
}
