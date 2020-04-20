using CalendarBot.Models;
using System.Collections.Generic;

namespace CalendarBot.Services
{
    public interface IConsultantParser
    {
        ICollection<Month> ParseCalendar(int year);
    }
}