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
