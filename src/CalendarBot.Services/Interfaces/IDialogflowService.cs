using System.Threading.Tasks;
using CalendarBot.Models;

namespace CalendarBot.Services
{
    public interface IDialogflowService
    {
        Task<Dialog> GetResponseAsync(Request request);
    }
}