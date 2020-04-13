using System.Linq;

namespace CalendarBot.Models.Internal
{
    public class AnswerTemplate : Payload
    {
        public Answer[] Answers { get; set; }

        public Answer this[DayType dayType]
        {
            get
            {
                return Answers.Where(d => d.DayType == dayType).FirstOrDefault();
            }
        }
    }
}
