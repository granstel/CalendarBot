using InstaParse.Parsers;

namespace CalendarBot.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            new ConsultantParser().ParseCalendar();
        }
    }
}
