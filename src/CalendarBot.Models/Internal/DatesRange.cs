﻿using System;

namespace CalendarBot.Models.Internal
{
    public class DatesRange
    {
        public DatesRange(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
    }
}
