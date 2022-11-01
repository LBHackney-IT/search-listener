using AutoFixture;
using FluentAssertions;
using Force.DeepCloner;
using Hackney.Core.Sns;
using Hackney.Shared.HousingSearch.Domain.Contract;
using Hackney.Shared.HousingSearch.Gateways.Models.Assets;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using HousingSearchListener.V1.UseCase;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using EventTypes = HousingSearchListener.V1.Boundary.EventTypes;

namespace HousingSearchListener.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class AddOrUpdateContractInAssetUseCaseTests
    {
        private readonly Mock<IAssetApiGateway> _mockAssetApi;
        private readonly Mock<IContractApiGateway> _mockContractApi;
        private readonly Mock<IEsGateway> _mockEsGateway;
        private readonly IESEntityFactory _esEntityFactory;
        private readonly AddOrUpdateContractInAssetUseCase _sut;

        private readonly EntityEventSns _messageCreated;
        private readonly EntityEventSns _messageAsset;
        private readonly Hackney.Shared.HousingSearch.Domain.Asset.Asset _Asset;
        private readonly Contract _Contract;

        private readonly Fixture _fixture;
        private static readonly Guid _correlationId = Guid.NewGuid();

        public AddOrUpdateContractInAssetUseCaseTests()
        {
            _fixture = new Fixture();

            _mockAssetApi = new Mock<IAssetApiGateway>();
            _mockContractApi = new Mock<IContractApiGateway>();
            _mockEsGateway = new Mock<IEsGateway>();
            _esEntityFactory = new ESEntityFactory();
            _sut = new AddOrUpdateContractInAssetUseCase(_mockEsGateway.Object,
                _mockContractApi.Object, _mockAssetApi.Object, _esEntityFactory);

            _messageAsset = CreateAssetMessage();
            _messageCreated = CreateContractCreatedEventMessage();
            _Asset = CreateAsset(_messageAsset.EntityId);
            _Contract = CreateContract(_messageCreated.EntityId);
        }

        private EntityEventSns CreateAssetMessage(string eventType = EventTypes.AssetCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private EntityEventSns CreateContractCreatedEventMessage(string eventType = EventTypes.ContractCreatedEvent)
        {
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EventType, eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .Create();
        }

        private Hackney.Shared.HousingSearch.Domain.Asset.Asset CreateAsset(Guid entityId)
        {
            return _fixture.Build<Hackney.Shared.HousingSearch.Domain.Asset.Asset>()
                           .With(x => x.Id, entityId.ToString())
                           .Create();
        }

        private Contract CreateContract(Guid entityId)
        {
            return _fixture.Build<Contract>()
                           .With(x => x.Id, entityId.ToString())
                           .Create();
        }

        private Guid? SetMessageEventData(Hackney.Shared.HousingSearch.Domain.Asset.Asset asset, EntityEventSns message, bool hasChanges, Contract added = null)
        {
            var oldData = asset;
            var newData = oldData.DeepClone();
            message.EventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "asset", oldData } },
                NewData = new Dictionary<string, object> { { "asset", newData } }
            };

            Guid? contractId = null;
            if (hasChanges)
            {
                if (added is null)
                {
                    var changed = newData;
                    changed.Contract.Charges.First().Amount = 90;
                    contractId = Guid.Parse(changed.Id);
                }
                else
                {
                    foreach (var charge in added.Charges)
                    {
                        Charges queryableCharge = new Charges();
                        queryableCharge.Id = charge.Id;
                        queryableCharge.Type = charge.Type;
                        queryableCharge.SubType = charge.SubType;
                        queryableCharge.Frequency = charge.Frequency;
                        queryableCharge.Amount = charge.Amount;
                        newData.Contract.Charges.ToList().Add(queryableCharge);
                    }

                    contractId = Guid.Parse(added.Id);
                }
            }
            return contractId;
        }

        private bool VerifyAssetIndexed(QueryableAsset esAsset)
        {
            esAsset.Should().BeEquivalentTo(_esEntityFactory.CreateAsset(_Asset));
            return true;
        }


        private bool VerifyContractIndexed(QueryableAsset esAsset, Contract contract, Hackney.Shared.HousingSearch.Domain.Asset.Asset asset)
        {
            esAsset.Should().BeEquivalentTo(_esEntityFactory.CreateAsset(asset));

            var newContract = esAsset.AssetContract;
            newContract.Should().NotBeNull();
            newContract.TargetId.Should().Be(contract.TargetId);
            newContract.TargetType.Should().Be(contract.TargetType);
            newContract.Charges.Should().AllBeEquivalentTo(contract.Charges);

            return true;
        }

        [Fact]
        public void ProcessMessageAsyncTestNullMessageThrows()
        {
            Func<Task> func = async () => await _sut.ProcessMessageAsync(null).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestGetContractExceptionThrown()
        {
            var exMsg = "This is an error";
            _mockContractApi.Setup(x => x.GetContractByIdAsync(_messageCreated.EntityId, _messageCreated.CorrelationId))
                                       .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_messageCreated).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);
        }

        [Fact]
        public void ProcessMessageAsyncTestGetContractReturnsNullThrows()
        {
            _mockContractApi.Setup(x => x.GetContractByIdAsync(_messageCreated.EntityId, _messageCreated.CorrelationId))
                                       .ReturnsAsync((Contract)null);

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_messageCreated).ConfigureAwait(false);
            func.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public void ProcessMessageAsyncTestIndexContractExceptionThrows()
        {
            var assetId = SetMessageEventData(_Asset, _messageAsset, true);
            var asset = CreateAsset(assetId.Value);

            _mockAssetApi.Setup(x => x.GetAssetByIdAsync(_messageAsset.EntityId, _messageAsset.CorrelationId))
                                       .ReturnsAsync(asset);
            var exMsg = "This is the last error";
            _mockEsGateway.Setup(x => x.IndexAsset(It.IsAny<QueryableAsset>()))
                          .ThrowsAsync(new Exception(exMsg));

            Func<Task> func = async () => await _sut.ProcessMessageAsync(_messageCreated).ConfigureAwait(false);
            func.Should().ThrowAsync<Exception>().WithMessage(exMsg);

            _mockEsGateway.Verify(x => x.IndexAsset(It.Is<QueryableAsset>(y => VerifyAssetIndexed(y))), Times.Never);
        }
    }
}
