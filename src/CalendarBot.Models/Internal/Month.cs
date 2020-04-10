using System.Collections.Generic;
using System.Linq;

namespace CalendarBot.Models.Internal
{
    public class Month
    {
        public Month(string name, int number, int year)
        {
            Name = name;
            Number = number;
            Year = year;

            Days = new List<Day>();
        }

        public string Name { get; }

        public int Number { get; }

        public int Year { get; }

        public IList<Day> Days { get; set; }

        public IDictionary<DayType, ICollection<DatesRange>> Ranges { get; set; }

        public IEnumerable<DatesRange> this[DayType dayType]
        {
            get
            {
                return Ranges.Where(d => d.Key == dayType).SelectMany(d => d.Value);
            }
        }
    }
}
