using AutoFixture;
using FluentAssertions;
using Hackney.Core.Http;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Gateway;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway
{
    [Collection("LogCall collection")]
    public class PersonApiGatewayTests
    {
        private readonly Mock<IApiGateway> _mockApiGateway;

        private static readonly Guid _id = Guid.NewGuid();
        private static readonly Guid _correlationId = Guid.NewGuid();

        private const string ApiName = "Person";
        private const string PersonApiUrlKey = "PersonApiUrl";
        private const string PersonApiTokenKey = "PersonApiToken";

        private const string PersonApiRoute = "https://some-domain.com/api/";
        private const string PersonApiToken = "dksfghjskueygfakseygfaskjgfsdjkgfdkjsgfdkjgf";

        public PersonApiGatewayTests()
        {
            _mockApiGateway = new Mock<IApiGateway>();

            _mockApiGateway.SetupGet(x => x.ApiName).Returns(ApiName);
            _mockApiGateway.SetupGet(x => x.ApiRoute).Returns(PersonApiRoute);
            _mockApiGateway.SetupGet(x => x.ApiToken).Returns(PersonApiToken);
        }

        private static string Route => $"{PersonApiRoute}/persons/{_id}";

        [Fact]
        public void ConstructorTestInitialisesApiGateway()
        {
            new PersonApiGateway(_mockApiGateway.Object);
            _mockApiGateway.Verify(x => x.Initialise(ApiName, PersonApiUrlKey, PersonApiTokenKey, null, false),
                                   Times.Once);
        }

        [Fact]
        public void GetPersonByIdAsyncGetExceptionThrown()
        {
            var exMessage = "This is an exception";
            _mockApiGateway.Setup(x => x.GetByIdAsync<Person>(Route, _id, _correlationId))
                           .ThrowsAsync(new Exception(exMessage));

            var sut = new PersonApiGateway(_mockApiGateway.Object);
            Func<Task<Person>> func =
                async () => await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            func.Should().ThrowAsync<Exception>().WithMessage(exMessage);
        }

        [Fact]
        public async Task GetPersonByIdAsyncNotFoundReturnsNull()
        {
            var sut = new PersonApiGateway(_mockApiGateway.Object);
            var result = await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPersonByIdAsyncCallReturnsPerson()
        {
            var person = new Fixture().Create<Person>();

            _mockApiGateway.Setup(x => x.GetByIdAsync<Person>(Route, _id, _correlationId))
                           .ReturnsAsync(person);

            var sut = new PersonApiGateway(_mockApiGateway.Object);
            var result = await sut.GetPersonByIdAsync(_id, _correlationId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(person);
        }
    }
}
