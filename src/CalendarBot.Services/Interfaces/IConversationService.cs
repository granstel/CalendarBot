using System.Threading.Tasks;
using CalendarBot.Models.Internal;

namespace CalendarBot.Services
{
    public interface IConversationService
    {
        Task<Response> GetResponseAsync(Request request);
    }
}