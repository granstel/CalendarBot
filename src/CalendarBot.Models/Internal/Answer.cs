namespace CalendarBot.Models.Internal
{
    public class Answer
    {
        public DayType DayType { get; set; }
       
        public string MainFormat { get; set; }

        public string RangeFormat { get; set; }

        public string EnumerationSeparator { get; set; }
        
        public string EmptyRangePhrase { get; set; }
    }
}
