using System;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Services;
using NLog;
using Yandex.Dialogs.Models;
using Internal = CalendarBot.Models.Internal;

namespace CalendarBot.Messengers.Yandex
{
    public class YandexService : MessengerService<InputModel, OutputModel>, IYandexService
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

        protected override Internal.Request Before(InputModel input)
        {
            if (input == default)
            {
                _log.Error("Input = null");

                input = CreateErrorInput();
            }

            return base.Before(input);
        }

        protected override Internal.Response ProcessCommand(Internal.Request request)
        {
            Internal.Response response = null;

            if (PingCommand.Equals(request?.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                response = new Internal.Response { Text = PongResponse };
            }

            if (ErrorCommand.Equals(request?.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                response = new Internal.Response { Text = "Простите, у меня какие-то проблемы..." };
            }

            return response;
        }

        protected override async Task<OutputModel> AfterAsync(InputModel input, Internal.Response response)
        {
            if (input == default)
            {
                input = CreateErrorInput();
            }

            var output = await base.AfterAsync(input, response);

            _mapper.Map(input, output);

            return output;
        }

        private InputModel CreateErrorInput()
        {
            return new InputModel
            {
                Request = new Request
                {
                    OriginalUtterance = ErrorCommand
                },
                Session = new InputSession(),
                Version = "1.0"
            };
        }
    }
}
