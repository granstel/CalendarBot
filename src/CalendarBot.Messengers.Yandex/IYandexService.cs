using CalendarBot.Services;
using Yandex.Dialogs.Models;
using Yandex.Dialogs.Models.Input;

namespace CalendarBot.Messengers.Yandex
{
    public interface IYandexService : IMessengerService<InputModel, OutputModel>
    {
    }
}