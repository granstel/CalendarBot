namespace CalendarBot.Models.Internal
{
    public class AnswerTemplate : Payload
    {
        public int Id { get; set; }

        public Answer[] Answers { get; set; }
    }
}
