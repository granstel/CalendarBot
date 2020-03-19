using CalendarBot.Services;
using Yandex.Dialogs.Models;

namespace CalendarBot.Messengers.Yandex
{
    public interface IYandexService : IMessengerService<InputModel, OutputModel>
    {
    }
}