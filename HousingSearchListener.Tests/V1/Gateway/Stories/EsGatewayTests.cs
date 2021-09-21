using HousingSearchListener.Tests.V1.Gateway.Fixtures;
using HousingSearchListener.Tests.V1.Gateway.Steps;
using System;
using TestStack.BDDfy;
using Xunit;

namespace HousingSearchListener.Tests.V1.Gateway.Stories
{
    [Story(
           AsA = "User",
           IWant = "A function to process the person and the tenure created and updated actions",
           SoThat = "The person and the tenure details are set in the index")]
    [Collection("ElasticSearch collection")]
    public class EsGatewayTests : IDisposable
    {
        private readonly EsGatewayFixture _esGatewayFixture;
        private readonly EsGatewaySteps _steps;

        public EsGatewayTests()
        {
            _esGatewayFixture = new EsGatewayFixture();
            _steps = new EsGatewaySteps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _esGatewayFixture.Dispose();

                _disposed = true;
            }
        }

        [Fact]
        public void PersonIsNullThrowsArgumentNullException()
        {
            this.Given(g => _esGatewayFixture.GivenTheEsPersonDoesNotExist())
                .When(w => _steps.WhenIndexPersonIsTriggered(null))
                .Then(t => _steps.ThenArgumentNullExceptionIsThrown())
                .BDDfy();
        }

        [Fact]
        public void PersonCreatedAddedToIndex()
        {
            this.Given(g => _esGatewayFixture.GivenTheEsPersonExists())
                .When(w => _steps.WhenIndexPersonIsTriggered(EsGatewayFixture.EsPerson))
                .Then(t => _steps.ThenAPersonCreated(EsGatewayFixture.EsPerson))
                .BDDfy();
        }

        [Fact]
        public void TenureIsNullThrowsArgumentNullException()
        {
            this.Given(g => _esGatewayFixture.GivenTheQueryableTenureDoesNotExist())
                .When(w => _steps.WhenIndexTenureIsTriggered(null))
                .Then(t => _steps.ThenArgumentNullExceptionIsThrown())
                .BDDfy();
        }

        [Fact]
        public void TenureCreatedAddedToIndex()
        {
            this.Given(g => _esGatewayFixture.GivenTheQueryableTenureExists())
                .When(w => _steps.WhenIndexTenureIsTriggered(EsGatewayFixture.QueryableTenure))
                .Then(t => _steps.ThenATenureCreated(EsGatewayFixture.QueryableTenure))
                .BDDfy();
        }

        [Fact]
        public void AddTenureToPersonIndexPersonExists()
        {
            this.Given(g => _esGatewayFixture.GivenTheEsPersonExists())
                .Given(g => _esGatewayFixture.GivenTheEsPersonTenureExists())
                .When(w => _steps.WhenPersonAlreadyExists(EsGatewayFixture.EsPerson))
                .When(w => _steps.WhenAddTenureToPersonIsTriggered(EsGatewayFixture.EsPerson, EsGatewayFixture.EsPersonTenure))
                .Then(t => _steps.ThenAPersonAccountAdded(EsGatewayFixture.EsPerson, EsGatewayFixture.EsPersonTenure))
                .BDDfy();
        }

        [Fact]
        public void AddTenureToPersonIndexPersonIsNullThrowsArgumentNullException()
        {
            this.Given(g => _esGatewayFixture.GivenTheEsPersonDoesNotExist())
                .Given(g => _esGatewayFixture.GivenTheEsPersonTenureExists())
                .When(w => _steps.WhenAddTenureToPersonIsTriggered(null, EsGatewayFixture.EsPersonTenure))
                .Then(t => _steps.ThenArgumentNullExceptionIsThrown())
                .BDDfy();
        }

        [Fact]
        public void AddTenureToPersonIndexTenureIsNullThrowsArgumentNullException()
        {
            this.Given(g => _esGatewayFixture.GivenTheEsPersonExists())
                .Given(g => _esGatewayFixture.GivenTheEsPersonTenureExists())
                .When(w => _steps.WhenPersonAlreadyExists(EsGatewayFixture.EsPerson))
                .When(w => _steps.WhenAddTenureToPersonIsTriggered(EsGatewayFixture.EsPerson, null))
                .Then(t => _steps.ThenArgumentNullExceptionIsThrown())
                .BDDfy();
        }
    }
}
