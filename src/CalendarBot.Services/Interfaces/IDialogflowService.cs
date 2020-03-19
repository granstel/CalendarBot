using System.Threading.Tasks;
using CalendarBot.Models.Internal;

namespace CalendarBot.Services
{
    public interface IDialogflowService
    {
        Task<Dialog> GetResponseAsync(Request request);
    }
}