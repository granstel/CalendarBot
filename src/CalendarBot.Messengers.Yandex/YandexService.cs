using System;
using System.Threading.Tasks;
using AutoMapper;
using CalendarBot.Models;
using CalendarBot.Services;
using NLog;
using Yandex.Dialogs.Models.Input;
using YandexModels = Yandex.Dialogs.Models;

namespace CalendarBot.Messengers.Yandex
{
    public class YandexService : MessengerService<InputModel, YandexModels.OutputModel>, IYandexService
    {
        private const string PingCommand = "ping";
        private const string PongResponse = "pong";
        private const string ErrorCommand = "error";

        private const string oldUSerStateKey = "isOldUser";

        private readonly Logger _log = LogManager.GetLogger(nameof(YandexService));

        private readonly IMapper _mapper;

        public YandexService(IConversationService conversationService, IMapper mapper) : base(conversationService, mapper)
        {
            _mapper = mapper;
        }

        protected override Request Before(InputModel input)
        {
            if (input == default)
            {
                _log.Error($"{nameof(InputModel)} is null");

                input = CreateErrorInput();
            }

            var result = base.Before(input);

            if (input.TryGetFromSessionState(oldUSerStateKey, out bool IsOldUser))
            {
                result.IsOldUser = IsOldUser;
            }

            return result;
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

        protected override async Task<YandexModels.OutputModel> AfterAsync(InputModel input, Response response)
        {
            if (input == default)
            {
                input = CreateErrorInput();
            }

            var output = await base.AfterAsync(input, response);

            _mapper.Map(input, output);

            output.AddToUserState(oldUSerStateKey, true);

            return output;
        }

        private InputModel CreateErrorInput()
        {
            return new InputModel
            {
                Request = new YandexModels.Request
                {
                    OriginalUtterance = ErrorCommand
                },
                Session = new InputSession(),
                Version = "1.0"
            };
        }
    }
}
