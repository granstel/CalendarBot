using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models.Internal;

namespace CalendarBot.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IDialogflowService _dialogflowService;
        private readonly IMapper _mapper;

        public ConversationService(IDialogflowService dialogflowService, IMapper mapper)
        {
            _dialogflowService = dialogflowService;
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
