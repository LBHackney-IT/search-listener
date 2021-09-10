using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class MissedEntityDataExceptionTest
    {
        [Fact]
        public void MissedEntityDataExceptionConstructorTest()
        {
            var message = "Test error.";

            var ex = new MissedEntityDataException(message);
            ex.Message.Should().Be(message);
        }
    }
}
