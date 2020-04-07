using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models.Internal;
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

            var datePeriodsString = dialog?.Parameters?.Where(p => string.Equals(p.Key, "date-period")).Select(p => p.Value).FirstOrDefault();

            var periodsStrings = datePeriodsString?.Split('/');

            var dates = periodsStrings?.Select(s =>
            {
                if (DateTime.TryParse(s, out var date))
                    return date;
                return default(DateTime?);
            }).Where(d => d.HasValue).OrderBy(d => d).ToList();

            var year = dates?.Select(d => d?.Year).FirstOrDefault();
            var month = dates?.Select(d => d?.Month).FirstOrDefault().GetValueOrDefault();

            if (!year.HasValue || !month.HasValue)
            {
                year = DateTime.Now.Year;
                month = DateTime.Now.Month;
            }
            _cache.TryGet($"Calendar:{year}", out Month[] calendar);

            //TODO: if no data at cache, send notification to parse

            var requestedDayTypes = dialog.Parameters?.Where(p => string.Equals(p.Key, "daytype")).SelectMany(p => p.Value.Split('/')).ToList();

            var daysForUserRequest = new Dictionary<DayType, ICollection<Day>>();

            var templates = dialog.Payloads.OfType<AnswerTemplate>().Where(t => string.Equals(t.Id, dialog.Response)).FirstOrDefault();

            var stringBuilder = new StringBuilder();

            foreach (var requestedDayType in requestedDayTypes)
            {
                if (!Enum.TryParse(requestedDayType, out DayType dayType))
                {
                    _log.Warn($"Can't parse \"{requestedDayType}\" to \"{nameof(DayType)}\" type");

                    continue;
                }

                if (dayType == DayType.Work)
                    continue;

                var template = templates[dayType];

                var ranges = calendar[month.Value - 1][dayType].ToList();

                var rangesString = FormatRanges(ranges, template);

                var joinedRanges = string.Join(template?.EnumerationDelimiter, rangesString);
                stringBuilder.Append(string.Format(template?.MainFormat ?? "{0}", joinedRanges));
                stringBuilder.AppendLine();
            }

            var response = new Response { Text = stringBuilder.ToString() };

            return response;
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
                    result.Add(string.Format(template?.RangeFormat, $"{startDay}го", $"{endDay}е"));
                }
            }

            return result;
        }
    }
}
