using System;

namespace CalendarBot.Services.Extensions
{
    public static class StringExtensions
    {
        private const string EncodedQuotes = "&quot;";

        public static string Sanitize(this string answer)
        {
            return answer?.Replace(EncodedQuotes, "\"");
        }

        public static DateTime GetDateOrDefault(this string source)
        {//TODO: test for null source
            if (DateTime.TryParse(source, out var dateTime))
                return dateTime.Date;

            return DateTime.Now.Date;
        }
    }
}
