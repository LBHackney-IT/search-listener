using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using System;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class TenureNotFoundExceptionTests
    {
        [Fact]
        public void TenureNotFoundExceptionConstructorTest()
        {
            var id = Guid.NewGuid();

            var ex = new TenureNotFoundException(id);
            ex.Id.Should().Be(id);
            ex.EntityName.Should().Be("Tenure");
            ex.Message.Should().Be($"Tenure with id {id} not found.");
        }
    }
}
