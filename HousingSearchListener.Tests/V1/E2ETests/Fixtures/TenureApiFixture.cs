using AutoFixture;
using Force.DeepCloner;
using Hackney.Core.Sns;
using Hackney.Core.Testing.Shared.E2E;
using HousingSearchListener.V1.Domain.Tenure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class TenureApiFixture : BaseApiFixture<TenureInformation>
    {
        private readonly Fixture _fixture = new Fixture();
        private const string DateFormat = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public EventData MessageEventData { get; private set; }
        public Guid AddedPersonId { get; private set; }
        public Guid RemovedPersonId { get; private set; }

        public TenureApiFixture()
            : base(FixtureConstants.TenureApiRoute, FixtureConstants.TenureApiToken)
        {
            Environment.SetEnvironmentVariable("TenureApiUrl", FixtureConstants.TenureApiRoute);
            Environment.SetEnvironmentVariable("TenureApiToken", FixtureConstants.TenureApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        private List<HouseholdMembers> CreateHouseholdMembers(int count = 3)
        {
            return _fixture.Build<HouseholdMembers>()
                           .With(x => x.Id, () => Guid.NewGuid().ToString())
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-40).ToString(DateFormat))
                           .With(x => x.PersonTenureType, "Tenant")
                           .CreateMany(count).ToList();
        }

        private void CreateMessageEventDataForPersonAdded(List<HouseholdMembers> hms = null)
        {
            var oldData = hms ?? CreateHouseholdMembers();
            var newData = oldData.DeepClone();

            var newHm = CreateHouseholdMembers(1).First();
            newHm.IsResponsible = true; //ensure newly added person is always the responsible one which makes them a tenant. This is the expected state in the index
            newData.Add(newHm);
            AddedPersonId = Guid.Parse(newHm.Id);

            MessageEventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
        }

        private void CreateMessageEventDataForPersonRemoved(Guid id)
        {
            var oldData = CreateHouseholdMembers();
            var newData = oldData.DeepClone();

            var removedHm = CreateHouseholdMembers(1).First();
            removedHm.Id = id.ToString();
            oldData.Add(removedHm);
            RemovedPersonId = id;

            MessageEventData = new EventData()
            {
                OldData = new Dictionary<string, object> { { "householdMembers", oldData } },
                NewData = new Dictionary<string, object> { { "householdMembers", newData } }
            };
        }

        public void GivenTheTenureDoesNotExist(Guid id)
        {
            CreateMessageEventDataForPersonAdded();
        }

        public void GivenAPersonWasRemoved(Guid id)
        {
            CreateMessageEventDataForPersonRemoved(id);
        }

        public TenureInformation GivenTheTenureExists(Guid id)
        {
            return GivenTheTenureExists(id, null);
        }
        public TenureInformation GivenTheTenureExists(Guid id, Guid? personId)
        {
            ResponseObject = _fixture.Build<TenureInformation>()
                                     .With(x => x.Id, id.ToString())
                                     .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-6).ToString(DateFormat))
                                     .With(x => x.EndOfTenureDate, DateTime.UtcNow.AddYears(6).ToString(DateFormat))
                                     .With(x => x.HouseholdMembers, CreateHouseholdMembers())
                                     .Create();

            if (personId.HasValue)
                ResponseObject.HouseholdMembers.First().Id = personId.ToString();

            CreateMessageEventDataForPersonAdded(ResponseObject.HouseholdMembers);
            return ResponseObject;
        }
    }
}
