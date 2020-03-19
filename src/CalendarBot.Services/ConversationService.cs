using System.Threading.Tasks;
using CalendarBot.Models.Internal;

namespace CalendarBot.Services
{
    public class ConversationService : IConversationService
    {
        public async Task<Response> GetResponseAsync(Request request)
        {
            //TODO: processing commands, invoking external services, and other cool asynchronous staff to generate response
            return await Task.Run(() => default(Response));
        }
    }
}
