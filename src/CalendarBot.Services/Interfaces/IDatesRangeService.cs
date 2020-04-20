using CalendarBot.Models;
using System.Collections.Generic;

namespace CalendarBot.Services
{
    public interface IDatesRangeService
    {
        IDictionary<DayType, ICollection<DatesRange>> GetRanges(Month month);
    }
}