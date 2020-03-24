using Autofac;
using CalendarBot.Services;
using CalendarBot.Services.Configuration;
using CalendarBot.Services.Parsers;
using CalendarBot.Services.Serialization;
using GranSteL.Helpers.Redis;
using StackExchange.Redis;

namespace CalendarBot.Api.DependencyModules
{
    public class InternalServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConversationService>().As<IConversationService>();
            builder.RegisterType<DialogflowService>().As<IDialogflowService>();
            builder.RegisterType<CustomJsonSerializer>().AsSelf();
            builder.RegisterType<HtmlParser>().As<IHtmlParser>();
            builder.Register(RegisterConsultantParser).As<IConsultantParser>();

            builder.Register(RegisterCacheService).As<IRedisCacheService>().SingleInstance();
        }

        private RedisCacheService RegisterCacheService(IComponentContext context)
        {
            var configuration = context.Resolve<RedisConfiguration>();

            var db = context.Resolve<IDatabase>();

            var service = new RedisCacheService(db, configuration.KeyPrefix);

            return service;
        }

        private object RegisterConsultantParser(IComponentContext context)
        {
            var htmlParser = context.Resolve<IHtmlParser>();
            var cache = context.Resolve<IRedisCacheService>();
            var configuration = context.Resolve<AppConfiguration>();

            var parser = new ConsultantParser(htmlParser, cache, configuration.CalendarSourceFormat);

            return parser;
        }
    }
}
