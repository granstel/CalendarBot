using System.Linq;

namespace CalendarBot.Models.Internal
{
    public class AnswerTemplate : Payload
    {
        public string Id { get; set; }

        public Answer[] Answers { get; set; }

        public Answer this[DayType dayType]
        {
            get
            {
                return Answers.Where(d => d.DayType == dayType).FirstOrDefault();
            }
        }

        public string NoYearInfoAnswer { get; set; }
    }
}
