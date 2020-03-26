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

        public IDictionary<string, Month> ParseCalendar(string year)
        {
            var _url = string.Format(_calendarSourceFormat, year);

            var document = _htmlParser.GetDocumentByUrl(_url);

            var calendars = document.DocumentNode.SelectNodes("//*[@class=\"cal\"]");

            var monthDictionary = new Dictionary<string, Month>();

            for (var i = 0; i < calendars.Count(); i++)
            {
                var calendar = calendars[i];

                var monthNode = calendar.SelectNodes($"{calendar.XPath}//th[@class=\"month\"]");

                var monthName = monthNode.Select(m => m.InnerText).FirstOrDefault();

                var month = new Month(monthName);

                if (monthName != null && !monthDictionary.ContainsKey(monthName))
                {
                    monthDictionary.Add(monthName, month);
                }

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


                var holidaysNodes = calendar.SelectNodes($"{calendar.XPath}//td[@class=\"holiday weekend\"]");

                var holidays = holidaysNodes?.Select(n => n.InnerText).ToList();

                if (holidays != null)
                    month.Holidays.AddRange(holidays);

                var preHolidaysNodes = calendar.SelectNodes($"{calendar.XPath}//td[@class=\"preholiday\"]");

                var preHolidays = preHolidaysNodes?.Select(n => n.InnerText).ToList();

                if (preHolidays != null)
                    month.PreHolidays.AddRange(preHolidays);

                var weekNodes = calendar.SelectNodes($"{calendar.XPath}//td[@class=\"weekend\"]");

                var weekends = weekNodes.Select(n => n.InnerText).ToList();

                if (weekNodes != null)
                    month.Weekends.AddRange(weekends);
            }

            try
            {
                _cache.Add($"Calendar:{year}", monthDictionary);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Can't add calendar for {year}");
            }

            return monthDictionary;
        }
    }
}
