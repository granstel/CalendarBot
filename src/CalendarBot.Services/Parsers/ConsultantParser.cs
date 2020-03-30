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
        private readonly Logger _log = LogManager.GetLogger(nameof(ConsultantParser));

        private readonly IHtmlParser _htmlParser;
        private readonly IRedisCacheService _cache;
        private readonly IDatesRangeService _rangeService;
        private readonly string _calendarSourceFormat;

        public ConsultantParser(IHtmlParser htmlParser, IRedisCacheService cache, IDatesRangeService rangeService, string calendarSourceFormat)
        {
            _htmlParser = htmlParser;
            _cache = cache;
            _rangeService = rangeService;
            _calendarSourceFormat = calendarSourceFormat;
        }

        public ICollection<Month> ParseCalendar(int year)
        {
            var _url = string.Format(_calendarSourceFormat, year);

            var document = _htmlParser.GetDocumentByUrl(_url);

            var htmlCalendars = document.DocumentNode.SelectNodes("//*[@class=\"cal\"]");

            var calendar = new List<Month>();

            for (var i = 0; i < htmlCalendars.Count(); i++)
            {
                var htmlCalendar = htmlCalendars[i];

                var monthNode = htmlCalendar.SelectNodes($"{htmlCalendar.XPath}//th[@class=\"month\"]");

                var monthName = monthNode.Select(m => m.InnerText).FirstOrDefault();

                var monthNumber = i + 1;

                var month = new Month(monthName, monthNumber, year);

                calendar.Add(month);

                var days = htmlCalendar.SelectNodes($"{htmlCalendar.XPath}//td");

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

                    if (day.Attributes.Any(a => a.Name == "class" && a.Value.Contains("holiday weekend")))
                    {
                        monthDay.Type = DayType.NotWork;

                        continue;
                    }

                    if (day.Attributes.Any(a => a.Name == "class" && a.Value.Contains("weekend")))
                    {
                        monthDay.Type = DayType.Weekend;

                        continue;
                    }

                }

                var ranges = _rangeService.GetRanges(month);
            }

            try
            {
                _cache.Add($"Calendar:{year}", calendar);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Can't add calendar for {year}");
            }

            return calendar;
        }
    }
}
