using Microsoft.AspNetCore.Mvc;
using Yandex.Dialogs.Models;

namespace CalendarBot.Messengers.Yandex
{
    [Produces("application/json")]
    [Route("/Yandex")]
    public class YandexController : MessengerController<InputModel, OutputModel>
    {
        public YandexController(IYandexService yandexService, YandexConfiguration configuration) : base(yandexService, configuration)
        {
        }
    }
}
