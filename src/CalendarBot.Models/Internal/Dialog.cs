using System.Collections.Generic;
using System.Linq;

namespace CalendarBot.Models.Internal
{
    public class Dialog
    {
        public IDictionary<string, string> Parameters { get; set; }

        public ICollection<Payload> Payloads { get; set; }

        public bool EndConversation { get; set; }

        public bool ParametersIncomplete { get; set; }

        public string Response { get; set; }

        public string Action { get; set; }

        public string IntentName { get; set; }

        public IEnumerable<string> GetParameters(string key)
        {
            return Parameters?.Where(p => string.Equals(p.Key, key)).Select(p => p.Value);
        }

        public IEnumerable<T> GetPayloads<T>(string key) where T : Payload
        {
            return Payloads?.OfType<T>().Where(t => string.Equals(t.Key, key));
        }
    }
}
