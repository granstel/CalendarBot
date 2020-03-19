using System.Threading.Tasks;
using CalendarBot.Models.Qna;

namespace CalendarBot.Services
{
    public interface IQnaClient
    {
        Task<Response> GetAnswerAsync(string knowledgeBase, string question);
    }
}