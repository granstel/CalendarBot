namespace CalendarBot.Models
{
    public enum DayType
    {
        Work,

        /// <summary>
        /// Short work day, usually before holidays
        /// </summary>
        PreHoliday,

        /// <summary>
        /// Holidays
        /// </summary>
        NotWork,

        /// <summary>
        /// Ordinary weekend, not holidays
        /// </summary>
        Weekend,

        /// <summary>
        /// Sudden weekend
        /// </summary>
        SuddenNotWork
    }
}
