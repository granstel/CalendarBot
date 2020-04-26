using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.Web;
using System;

namespace CalendarBot.Api
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception e)
            {
                Log.Error(e);

                throw;
            }
            finally
            {
                Zidium.Api.Client.Instance.EventManager.Flush();
                Zidium.Api.Client.Instance.WebLogManager.Flush();

                LogManager.Shutdown();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseNLog()
                .Build();
    }
}
