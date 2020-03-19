using System.Collections.Generic;

namespace InstaParse
{
    public class Month
    {
        public Month(string name)
        {
            Name = name;
            Holidays = new List<string>();
            PreHolidays = new List<string>();
            Weekends = new List<string>();
        }
        public string Name { get; }

        public List<string> Holidays { get; set; }
        public List<string> PreHolidays { get; set; }
        public List<string> Weekends { get; set; }
    }
}
