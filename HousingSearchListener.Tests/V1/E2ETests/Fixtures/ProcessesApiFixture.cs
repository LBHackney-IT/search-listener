using AutoFixture;
using Hackney.Core.Testing.Shared.E2E;
using Hackney.Shared.HousingSearch.Domain.Process;
using Hackney.Shared.Processes.Domain;
using System;
using Process = Hackney.Shared.Processes.Domain.Process;


namespace HousingSearchListener.Tests.V1.E2ETests.Fixtures
{
    public class ProcessesApiFixture : BaseApiFixture<Process>
    {
        private readonly Fixture _fixture = new Fixture();

        public ProcessesApiFixture()
            : base(FixtureConstants.ProcessesApiRoute, FixtureConstants.ProcessesApiToken)
        {
            Environment.SetEnvironmentVariable("ProcessesApiUrl", FixtureConstants.ProcessesApiRoute);
            Environment.SetEnvironmentVariable("ProcessesApiToken", FixtureConstants.ProcessesApiToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                base.Dispose(disposing);
            }
        }

        public void GivenTheProcessDoesNotExist(Guid id)
        {
            // Nothing to do here
        }

        public Process GivenTheProcessExists(Guid id)
        {
            ResponseObject = _fixture.Build<Process>()
                                     .With(x => x.Id, id)
                                     .Create();

            return ResponseObject;
        }

        public Process GivenTheProcessExists(Guid id, Guid targetId, TargetType targetType)
        {
            ResponseObject = _fixture.Build<Process>()
                                     .With(x => x.Id, id)
                                     .With(x => x.TargetId, targetId)
                                     .With(x => x.TargetType, targetType)
                                     .Create();

            return ResponseObject;
        }
    }
}
