using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models.Internal;
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
                    response = new Response { Text = dialog.Response };
                    break;
            }

            return response;
        }

        private Response GetDatesPeriodsReponse(Dialog dialog)
        {
            var datePeriodsString = dialog?.GetParameters("date-period")?.FirstOrDefault();

            var requestedDate = (datePeriodsString?.Split('/').Select(s => s.GetDateOrDefault())).FirstOrDefault();

            var year = requestedDate.Year;
            var month = requestedDate.Month;

            var templates = dialog?.GetPayloads<AnswerTemplate>(dialog.Response)?.FirstOrDefault();
            
            if (!_cache.TryGet($"Calendar:{year}", out Month[] calendar, true))
            {
                RunPrasing(year);

                var answer = string.Format(templates.NoYearInfoAnswer, year);

                return new Response { Text = answer };
            }

            var dayTypes = GetDayTypes(dialog);

            var stringBuilder = new StringBuilder();

            foreach (var dayType in dayTypes)
            {
                var template = templates[dayType];

                var ranges = calendar[month][dayType].ToList();

                var rangesString = FormatRanges(ranges, template);

                var joinedRanges = string.Join(template?.EnumerationSeparator, rangesString);

                stringBuilder.Append(string.Format(template?.MainFormat ?? "{0}", joinedRanges));
                stringBuilder.AppendLine();
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

            var templates = dialog?.GetPayloads<AnswerTemplate>(dialog.Response).FirstOrDefault();

            if (!_cache.TryGet($"Calendar:{year}", out Month[] calendar, true))
            {
                RunPrasing(year);

                var answer = string.Format(templates.NoYearInfoAnswer, year);

                return new Response { Text = answer };
            }

            var dayType = calendar[month].Days[day].Type;

            var template = templates[dayType];

            var mainFormat = template?.MainFormat ?? $"dd MMMM{0}: dddd, это {dayType}... Простите, я немного не в себе...";

            var yearFormat = string.Empty;

            if (year != DateTime.Now.Year)
            {
                yearFormat = $" {year}го года";
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

                if (range.EndDate < DateTime.Now)
                    continue;

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
    }
}
