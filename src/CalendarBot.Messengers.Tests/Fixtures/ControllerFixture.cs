using CalendarBot.Services;
using CalendarBot.Services.Configuration;

namespace CalendarBot.Messengers.Tests.Fixtures
{
    public class ControllerFixture : MessengerController<InputFixture, OutputFixture>
    {
        public ControllerFixture(IMessengerService<InputFixture, OutputFixture> messengerService, MessengerConfiguration configuration) : base(messengerService, configuration)
        {
        }
    }
}
