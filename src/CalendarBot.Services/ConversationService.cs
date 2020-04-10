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
        private readonly IRedisCacheService _cache;
        private readonly IMapper _mapper;

        public ConversationService(IDialogflowService dialogflowService, IRedisCacheService cache, IMapper mapper)
        {
            _dialogflowService = dialogflowService;
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
            var datePeriodsString = dialog?.Parameters?.Where(p => string.Equals(p.Key, "date-period")).Select(p => p.Value).FirstOrDefault();

            var periodsStrings = datePeriodsString?.Split('/');

            var dates = periodsStrings?.Select(s =>
            {
                if (DateTime.TryParse(s, out var date))
                    return date;
                return default(DateTime?);
            }).Where(d => d.HasValue).OrderBy(d => d).ToList();

            var year = dates?.Select(d => d?.Year).FirstOrDefault() ?? DateTime.Now.Year;
            var month = dates?.Select(d => d?.Month).FirstOrDefault() ?? DateTime.Now.Month;

            _cache.TryGet($"Calendar:{year}", out Month[] calendar, true);

            //TODO: if no data at cache, send notification to parse

            var requestedDayTypes = dialog.Parameters?.Where(p => string.Equals(p.Key, "daytype")).SelectMany(p => p.Value.Split('/')).ToList();

            var daysForUserRequest = new Dictionary<DayType, ICollection<Day>>();

            var templates = dialog.Payloads.OfType<AnswerTemplate>().Where(t => string.Equals(t.Id, dialog.Response)).FirstOrDefault();

            var stringBuilder = new StringBuilder();

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

            foreach (var dayType in dayTypes)
            {
                var template = templates[dayType];

                var ranges = calendar[month - 1][dayType].ToList();

                var rangesString = FormatRanges(ranges, template);

                var joinedRanges = string.Join(template?.EnumerationSeparator, rangesString);

                stringBuilder.Append(string.Format(template?.MainFormat ?? "{0}", joinedRanges));
                stringBuilder.AppendLine();
            }

            var response = new Response { Text = stringBuilder.ToString() };

            return response;
        }
        
        private Response GetDatesReponse(Dialog dialog)
        {
            var requestedDate = dialog?.Parameters?.Where(p => string.Equals(p.Key, "date")).Select(p => p.Value).Select(s =>
            {
                if (DateTime.TryParse(s, out var parsedDate))
                    return parsedDate;
                return default(DateTime?);
            }).FirstOrDefault() ?? DateTime.Now;

            var year = requestedDate.Year;
            var month = requestedDate.Month;
            var day = requestedDate.Day;

            _cache.TryGet($"Calendar:{year}", out Month[] calendar, true);

            var dayType = calendar[month - 1].Days[day - 1].Type;

            var info = requestedDate.ToRussianString($"dd MMMM: dddd, {dayType}");

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
                    result.Add(string.Format(template?.RangeFormat, $"{startDay}го", $"{endDay}е"));
                }
            }

            if (!result.Any())
            {
                return new[] { template.EmptyRangePhrase };
            }

            return result;
        }
    }
}
