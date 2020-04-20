using System.Threading.Tasks;
using CalendarBot.Models;

namespace CalendarBot.Services
{
    public interface IConversationService
    {
        Task<Response> GetResponseAsync(Request request);
    }
}