﻿namespace CalendarBot.Services.Configuration
{
    public class AppConfiguration
    {
        public HttpLogConfiguration HttpLog { get; set; }

        public QnaConfiguration Qna { get; set; }

        public DialogflowConfiguration Dialogflow { get; set; }

        public RedisConfiguration Redis { get; set; }

        public string CalendarSourceFormat { get; set; }
    }
}
