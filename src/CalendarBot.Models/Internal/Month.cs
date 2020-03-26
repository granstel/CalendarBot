using System.Collections.Generic;
using System.Linq;

namespace CalendarBot.Models.Internal
{
    public class Month
    {
        public Month(string name, int number)
        {
            Name = name;
            Number = number;

            Days = new List<Day>();
        }

        public string Name { get; }

        public int Number { get; }

        public ICollection<Day> Days { get; set; }

        public IEnumerable<Day> this[DayType dayType]
        {
            get
            {
                return Days.Where(d => d.Type == dayType);
            }
        }
    }
}
