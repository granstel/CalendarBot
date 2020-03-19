using AutoMapper;
using CalendarBot.Services.Mapping;
using NUnit.Framework;

namespace CalendarBot.Services.Tests.MappingProfiles
{
    [TestFixture]
    public class DialogflowProfileTests
    {
        private IMapper _target;

        [SetUp]
        public void InitTest()
        {
            _target = new Mapper(new MapperConfiguration(c => c.AddProfile<DialogflowProfile>()));
        }

        [Test]
        public void ValidateConfiguration_Success()
        {
            _target.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
