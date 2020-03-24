using CalendarBot.Models.Internal;
using System.Collections.Generic;

namespace CalendarBot.Services
{
    public interface IConsultantParser
    {
        IDictionary<string, Month> ParseCalendar(string year);
    }
}