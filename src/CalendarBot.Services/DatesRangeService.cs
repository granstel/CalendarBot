using CalendarBot.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CalendarBot.Services
{
    public class DatesRangeService : IDatesRangeService
    {
        public IDictionary<DayType, ICollection<DatesRange>> GetRanges(Month month)
        {
            var groups = month.Days.OrderBy(d => d.Number).GroupBy(d => d.Type).ToDictionary(d => d.Key, d => d.Select(g => g.Number).ToList());

            var result = new Dictionary<DayType, ICollection<DatesRange>>();

            foreach (var group in groups)
            {
                var dayType = group.Key;

                if (!result.ContainsKey(dayType))
                {
                    result.Add(dayType, new List<DatesRange>());
                }

                var groupDays = group.Value;

                var ranges = new List<List<int>>();

                var currentRange = new List<int>();
                ranges.Add(currentRange);

                Action createNewCurrentRange = () =>
                {
                    currentRange = new List<int>();

                    ranges.Add(currentRange);
                };

                for (var i = 0; i < groupDays.Count(); i++)
                {
                    var day = groupDays[i];

                    var nextDayIndex = i + 1;

                    if (i > 0)
                    {
                        var previousDay = groupDays[i - 1];

                        if (day - previousDay > 1)
                        {
                            createNewCurrentRange();
                        }
                    }

                    if (nextDayIndex >= groupDays.Count())
                    {
                        currentRange.Add(day);

                        //createNewCurrentRange();

                        continue;
                    }

                    var nextDay = groupDays[nextDayIndex];

                    if (nextDay - day == 1)
                    {
                        currentRange.AddRange(new[] { day, nextDay });

                        i++;
                    }
                    else
                    {
                        currentRange.Add(day);

                        createNewCurrentRange();
                    }
                }

                foreach (var range in ranges)
                {
                    if (!range.Any())
                        continue;

                    var minDateNumber = range.Min();
                    var maxDateNumber = range.Max();

                    var minDate = new DateTime(month.Year, month.Number, minDateNumber);
                    var maxDate = new DateTime(month.Year, month.Number, maxDateNumber);

                    var datesRange = new DatesRange(minDate, maxDate);

                    result[dayType].Add(datesRange);
                }
            }

            return result;
        }
    }
}
