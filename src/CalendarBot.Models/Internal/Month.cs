using System.Collections.Generic;

namespace CalendarBot.Models.Internal
{
    public class Month
    {
        public Month(string name)
        {
            Name = name;
            Days = new List<Day>();
            Holidays = new List<string>();
            PreHolidays = new List<string>();
            Weekends = new List<string>();
        }
        public string Name { get; }

        public List<Day> Days { get; set; }

        public List<string> Holidays { get; set; }
        public List<string> PreHolidays { get; set; }
        public List<string> Weekends { get; set; }
    }
}
