using System.Threading.Tasks;

namespace CalendarBot.Services
{
    public interface IQnaService
    {
        Task<string> GetAnswerAsync(string question);
    }
}