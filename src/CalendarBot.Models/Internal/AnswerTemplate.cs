﻿using System.Linq;

namespace CalendarBot.Models.Internal
{
    public class AnswerTemplate
    {
        public string Key { get; set; }
        public Answer[] Answers { get; set; }
        public string NoYearInfoAnswer { get; set; }
        public string YearFormat { get; set; }

        public Answer this[DayType dayType]
        {
            get
            {
                return Answers.Where(d => d.DayType == dayType).FirstOrDefault();
            }
        }
    }
}
