using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models.Internal;
using GranSteL.Helpers.Redis;

namespace CalendarBot.Services
{
    public class ConversationService : IConversationService
    {
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

            if(!string.IsNullOrEmpty(datePeriodsString))
            {
                var periodsStrings = datePeriodsString.Split('/');

                var dates = periodsStrings.Select(s =>
                {
                    if (DateTime.TryParse(s, out var date))
                        return date;
                    return default(DateTime?);
                }).Where(d => d.HasValue).OrderBy(d => d).ToList();

                var year = dates.Select(d => d?.Year).FirstOrDefault();

                if(year.HasValue)
                {
                    _cache.TryGet($"Calendar:{year}", out Dictionary<string, Month> calendar);
                }
            }

            var response = new Response { Text = dialog.Response };

            return response;
        }
    }
}
