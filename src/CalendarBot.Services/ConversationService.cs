using System;
using System.Collections.Generic;
using System.Linq;
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

            if (!string.IsNullOrEmpty(datePeriodsString))
            {
                var periodsStrings = datePeriodsString.Split('/');

                var dates = periodsStrings.Select(s =>
                {
                    if (DateTime.TryParse(s, out var date))
                        return date;
                    return default(DateTime?);
                }).Where(d => d.HasValue).OrderBy(d => d).ToList();

                var year = dates.Select(d => d?.Year).FirstOrDefault();
                var month = dates.Select(d => d?.Month).FirstOrDefault();

                if (year.HasValue)
                {
                    _cache.TryGet($"Calendar:{year}", out Month[] calendar);

                    //TODO: if no data at cache, send notification to parse

                    var requestedDayTypes = dialog.Parameters?.Where(p => string.Equals(p.Key, "daytype")).SelectMany(p => p.Value.Split('/')).ToList();

                    foreach(var requestedDayType in requestedDayTypes)
                    {
                        if (!Enum.TryParse(requestedDayType, out DayType dayType))
                        {
                            _log.Warn($"Can't parse \"{requestedDayType}\" to \"{nameof(DayType)}\" type");

                            continue;
                        }

                        var days = calendar[month.GetValueOrDefault() - 1][dayType];
                    }

                    
                }
            }

            var response = new Response { Text = dialog.Response };

            return response;
        }
    }
}
