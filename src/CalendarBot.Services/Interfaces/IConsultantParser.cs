using CalendarBot.Models.Internal;
using System.Collections.Generic;

namespace CalendarBot.Services
{
    public interface IConsultantParser
    {
        ICollection<Month> ParseCalendar(string year);
    }
}