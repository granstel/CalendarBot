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

            var response = new Response { Text = dialog.Response };

            return response;
        }
    }
}
