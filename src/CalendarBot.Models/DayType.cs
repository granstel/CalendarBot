namespace CalendarBot.Models
{
    public enum DayType
    {
        Work,

        //Short work day, usually before holidays
        PreHoliday,

        NotWork,

        /// <summary>
        /// Ordinary weekend, not holidays
        /// </summary>
        Weekend
    }
}
