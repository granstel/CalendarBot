using System;
using System.Threading.Tasks;
using CalendarBot.Services;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace CalendarBot.Api.Controllers
{
    /// <summary>
    /// Контроллер для обслуживания
    /// </summary>
    [Produces("application/json")]
    [Route("/Servicing")]
    public class ServicingController : Controller
    {
        private readonly Logger _log = LogManager.GetLogger(nameof(ServicingController));
        private readonly IConsultantParser _consultantParser;

        public ServicingController(IConsultantParser consultantParser)
        {
            _consultantParser = consultantParser;
        }

        [HttpPost("Parse/{year}")]
        public IActionResult ParseCalendar(string year)
        {
            var calendar = _consultantParser.ParseCalendar(year);

            return Json(calendar);
        }
    }
}
