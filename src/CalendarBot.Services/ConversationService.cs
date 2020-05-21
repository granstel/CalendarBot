using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models;
using CalendarBot.Services.Extensions;
using GranSteL.Helpers.Redis;
using NLog;

namespace CalendarBot.Services
{
    public class ConversationService : IConversationService
    {
        private readonly Logger _log = LogManager.GetLogger(nameof(ConversationService));

        private readonly IDialogflowService _dialogflowService;
        private readonly IConsultantParser _consultantParser;
        private readonly IRedisCacheService _cache;
        private readonly IMapper _mapper;

        public ConversationService(IDialogflowService dialogflowService, IConsultantParser consultantParser, IRedisCacheService cache, IMapper mapper)
        {
            _dialogflowService = dialogflowService;
            _consultantParser = consultantParser;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<Response> GetResponseAsync(Request request)
        {
            var dialog = await _dialogflowService.GetResponseAsync(request);

            Response response;
            switch (dialog.IntentName)
            {
                case "DatesPeriods":
                    response = GetDatesPeriodsReponse(dialog);
                    break;
                case "Dates":
                    response = GetDatesReponse(dialog);
                    break;
                default:
                    response = new Response { Text = dialog.Response, Finished = dialog.EndConversation };
                    break;
            }

            return response;
        }

        private Response GetDatesPeriodsReponse(Dialog dialog)
        {
            var datePeriodsString = dialog?.GetParameters("date-period")?.FirstOrDefault();

            var requestedDate = (datePeriodsString?.Split('/')).FirstOrDefault();

            var templates = dialog?.GetTemplate(dialog.Response);

            var dayTypes = GetDayTypes(dialog);

            var year = requestedDate.GetDateOrDefault().Year;
            var monthNumber = requestedDate.GetDateOrDefault().Month;

            var calendar = GetCalendar(year);

            if (calendar == null)
            {
                var answer = string.Format(templates.NoYearInfoAnswer, year);

                return new Response { Text = answer };
            }

            if (string.IsNullOrEmpty(requestedDate))
            {
                return GetClosestDaysResponse(calendar, templates, dayTypes);
            }

            return GetRangeResponse(calendar, templates, year, monthNumber, dayTypes);

        }

        private Response GetRangeResponse(Month[] calendar, AnswerTemplate templates, int year, int monthNumber, ICollection<DayType> dayTypes)
        {

            var month = calendar[monthNumber];
            var monthName = month.Name;

            var yearFormat = string.Empty;
            if (year != DateTime.Now.Year)
            {
                yearFormat = string.Format(templates.YearFormat, year);

                monthName = $"{monthName}{yearFormat}";
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format(templates.Introduction ?? "{0}", monthName));
            stringBuilder.AppendLine();

            foreach (var dayType in dayTypes)
            {
                var template = templates[dayType];

                var ranges = calendar[monthNumber][dayType].ToList();

                var rangesString = FormatRanges(ranges, template);

                var joinedRanges = string.Join(template?.EnumerationSeparator, rangesString);

                stringBuilder.Append(string.Format(template?.MainFormat ?? "{0}", joinedRanges));
                stringBuilder.AppendLine();
            }

            var text = stringBuilder.ToString();

            var image = GetImage(year, monthNumber, templates?.ImageTitleFormat, text);

            return new Response { Text = text, Image = image };
        }

        private Response GetClosestDaysResponse(Month[] calendar, AnswerTemplate templates, ICollection<DayType> dayTypes)
        {
            var stringBuilder = new StringBuilder();

            var monthNumber = DateTime.Now.Month;
            var nowDate = DateTime.Now.Date;

            var introduction = templates.ClosestIntroduction ?? "{0}";

            foreach (var dayType in dayTypes)
            {
                var template = templates[dayType];
                var mainFormat = string.Format(introduction, template.MainFormat.ToLower());

                var ranges = calendar.Where(c => c.Number >= monthNumber).SelectMany(c => c[dayType]).Where(r => r.StartDate >= DateTime.Now).Take(1).ToList();

                var rangesString = FormatRanges(ranges, template);

                //The name of the month in the case. Date formatting gives you the correct month name. Convenient, no need to go to other services
                var monthNameInCase = ranges.FirstOrDefault()?.StartDate.ToString("d MMMM").Split(' ').LastOrDefault();

                var joinedRanges = string.Join(template?.EnumerationSeparator, rangesString);

                var rangeWithMonthName = $"{joinedRanges} {monthNameInCase}";

                stringBuilder.Append(string.Format(mainFormat ?? "{0}", rangeWithMonthName));
                stringBuilder.AppendLine();

                introduction = "{0}";
            }

            var text = stringBuilder.ToString();

            return new Response { Text = text };
        }

        private Response GetDatesReponse(Dialog dialog)
        {
            var requestedDate = (dialog?.GetParameters("date").Select(s => s.GetDateOrDefault())).FirstOrDefault();

            var year = requestedDate.Year;
            var month = requestedDate.Month;
            var day = requestedDate.Day;

            var templates = dialog?.GetTemplate(dialog.Response);

            var calendar = GetCalendar(year);

            if (calendar == null)
            {
                var answer = string.Format(templates.NoYearInfoAnswer, year);

                return new Response { Text = answer };
            }

            var dayType = calendar[month].Days[day].Type;

            var template = templates[dayType];

            var mainFormat = template?.MainFormat ?? $"d MMMM{0}: dddd, это {dayType}... Простите, я немного не в себе...";

            var yearFormat = string.Empty;

            if (year != DateTime.Now.Year)
            {
                yearFormat = string.Format(templates.YearFormat, year);
            }

            var responseFormat = string.Format(mainFormat, yearFormat);

            var info = requestedDate.ToRussianString(responseFormat);

            return new Response { Text = info };
        }

        private ICollection<string> FormatRanges(List<DatesRange> ranges, Answer template)
        {
            var result = new List<string>();

            foreach (var range in ranges)
            {
                var startDay = range.StartDate.Day;
                var endDay = range.EndDate.Day;

                if (startDay == endDay)
                {
                    result.Add($"{startDay}го");
                }
                else
                {
                    result.Add(string.Format(template?.RangeFormat ?? "с {0} по {1}", $"{startDay}го", $"{endDay}е"));
                }
            }

            if (!result.Any())
            {
                return new[] { template.EmptyRangePhrase };
            }

            return result;
        }

        private ICollection<DayType> GetDayTypes(Dialog dialog)
        {
            var requestedDayTypes = dialog.GetParameters("daytype").SelectMany(p => p.Split('/')).ToList();

            var dayTypes = new List<DayType>();

            foreach (var requestedDayType in requestedDayTypes)
            {
                if (!Enum.TryParse(requestedDayType, out DayType dayType))
                {
                    _log.Warn($"Can't parse \"{requestedDayType}\" to \"{nameof(DayType)}\" type");

                    continue;
                }

                if (dayType == DayType.Work)
                    continue;

                dayTypes.Add(dayType);
            }

            if (!dayTypes.Any())
            {
                dayTypes.AddRange(new[] { DayType.PreHoliday, DayType.NotWork });
            }

            return dayTypes;
        }

        private void RunPrasing(int year)
        {
            Task.Run(() => _consultantParser.ParseCalendar(year)).Forget();
        }

        private Image GetImage(int year, int monthNumber, string imageTitleFormat, string description)
        {
            if (!_cache.TryGet($"Calendar:{year}:images:{monthNumber}", out string imageId))
                return null;

            var image = new Image
            {
                ImageId = imageId,
                Title = new DateTime(year, monthNumber, 1).ToString(imageTitleFormat ?? "MMMM"),
                Description = description
            };

            return image;
        }

        private Month[] GetCalendar(int year)
        {
            if (!_cache.TryGet($"Calendar:{year}", out Month[] calendar, true))
            {
                RunPrasing(year);
            }

            return calendar;
        }
    }
}
