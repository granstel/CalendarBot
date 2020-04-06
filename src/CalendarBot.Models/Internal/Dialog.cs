using System.Collections.Generic;

namespace CalendarBot.Models.Internal
{
    public class Dialog
    {
        public IDictionary<string, string> Parameters { get; set; }
        
        public ICollection<string> Payloads { get; set; }

        public bool EndConversation { get; set; }

        public bool ParametersIncomplete { get; set; }

        public string Response { get; set; }

        public string Action { get; set; }
    }
}
