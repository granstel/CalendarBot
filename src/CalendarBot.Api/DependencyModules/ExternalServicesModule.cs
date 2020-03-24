using Autofac;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using CalendarBot.Services.Configuration;
using Grpc.Auth;
using RestSharp;
using StackExchange.Redis;

namespace CalendarBot.Api.DependencyModules
{
    public class ExternalServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RestClient>().As<IRestClient>();
            
            builder.Register(RegisterDialogflowClient).As<SessionsClient>();

            builder.Register(RegisterRedisClient).As<IDatabase>().SingleInstance();
        }

        private SessionsClient RegisterDialogflowClient(IComponentContext context)
        {
            var configuration = context.Resolve<DialogflowConfiguration>();

            var credential = GoogleCredential.FromFile(configuration.JsonPath).CreateScoped(SessionsClient.DefaultScopes);

            var channel = new Grpc.Core.Channel(SessionsClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());

            var client = SessionsClient.Create(channel);

            return client;
        }

        private IDatabase RegisterRedisClient(IComponentContext context)
        {
            var configuration = context.Resolve<RedisConfiguration>();

            var redisClient = ConnectionMultiplexer.Connect(configuration.ConnectionString);

            var dataBase = redisClient.GetDatabase();

            return dataBase;
        }
    }
}
