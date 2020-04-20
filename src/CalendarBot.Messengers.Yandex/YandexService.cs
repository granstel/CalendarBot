using System;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models;
using CalendarBot.Services;
using NLog;
using YandexModels = Yandex.Dialogs.Models;

namespace CalendarBot.Messengers.Yandex
{
    public class YandexService : MessengerService<YandexModels.InputModel, YandexModels.OutputModel>, IYandexService
    {
        private const string PingCommand = "ping";
        private const string PongResponse = "pong";
        private const string ErrorCommand = "error";

        private readonly Logger _log = LogManager.GetLogger(nameof(YandexService));

        private readonly IMapper _mapper;

        public YandexService(IConversationService conversationService, IMapper mapper) : base(conversationService, mapper)
        {
            _mapper = mapper;
        }

        protected override Request Before(YandexModels.InputModel input)
        {
            if (input == default)
            {
                _log.Error("Input = null");

                input = CreateErrorInput();
            }

            return base.Before(input);
        }

        protected override Response ProcessCommand(Request request)
        {
            Response response = null;

            if (PingCommand.Equals(request?.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                response = new Response { Text = PongResponse };
            }

            if (ErrorCommand.Equals(request?.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                response = new Response { Text = "Простите, у меня какие-то проблемы..." };
            }

            return response;
        }

        protected override async Task<YandexModels.OutputModel> AfterAsync(YandexModels.InputModel input, Response response)
        {
            if (input == default)
            {
                input = CreateErrorInput();
            }

            var output = await base.AfterAsync(input, response);

            _mapper.Map(input, output);

            return output;
        }

        private YandexModels.InputModel CreateErrorInput()
        {
            return new YandexModels.InputModel
            {
                Request = new YandexModels.Request
                {
                    OriginalUtterance = ErrorCommand
                },
                Session = new YandexModels.InputSession(),
                Version = "1.0"
            };
        }
    }
}
