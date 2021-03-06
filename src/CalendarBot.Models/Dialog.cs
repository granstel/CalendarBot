﻿using System.Collections.Generic;
using System.Linq;

namespace CalendarBot.Models
{
    public class Dialog
    {
        public IDictionary<string, string> Parameters { get; set; }

        public ICollection<AnswerTemplate> AnswerTemplates { get; set; }

        public bool EndConversation { get; set; }

        public bool ParametersIncomplete { get; set; }

        public string Response { get; set; }

        public string Action { get; set; }

        public string IntentName { get; set; }

        public IEnumerable<string> GetParameters(string key)
        {
            return Parameters?.Where(p => string.Equals(p.Key, key)).Select(p => p.Value);
        }

        public AnswerTemplate GetTemplate(string key)
        {
            return AnswerTemplates?.Where(t => string.Equals(t.Key, key)).FirstOrDefault();
        }
    }
}
