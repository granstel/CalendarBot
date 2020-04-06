namespace CalendarBot.Models.Internal
{
    public class AnswerTemplate : Payload
    {
        public string DayType { get; set; }

        public Answer Answer { get; set; }

        public int Id { get; set; }
    }
}
