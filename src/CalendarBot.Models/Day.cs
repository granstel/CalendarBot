namespace CalendarBot.Models
{
    public class Day
    {
        public Day(int number)
        {
            Number = number;
        }

        public int Number { get; set; }

        public DayType Type { get; set; }

    }
}
