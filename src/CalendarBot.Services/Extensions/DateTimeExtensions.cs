using System;
using System.Globalization;

namespace CalendarBot.Services.Extensions
{
    public static class DateTimeExtensions
    {
        private static CultureInfo RussianCulture = CultureInfo.CreateSpecificCulture("ru-Ru");

        public static string ToRussianString(this DateTime source, string format)
        {
            return source.ToString(format, RussianCulture);
        }
    }
}
