using System.Collections.Generic;

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
    }
}
