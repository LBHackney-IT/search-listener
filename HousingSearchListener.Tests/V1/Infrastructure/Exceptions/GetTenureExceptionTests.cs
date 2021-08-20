using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using System;
using System.Net;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class GetTenureExceptionTests
    {
        [Fact]
        public void GetTenureExceptionConstructorTest()
        {
            var tenureId = Guid.NewGuid();
            var statusCode = HttpStatusCode.OK;
            var msg = "Some API error message";

            var ex = new GetTenureException(tenureId, statusCode, msg);
            ex.TenureId.Should().Be(tenureId);
            ex.StatusCode.Should().Be(statusCode);
            ex.ResponseBody.Should().Be(msg);
            ex.Message.Should().Be($"Failed to get tenure details for id {tenureId}. Status code: {statusCode}; Message: {msg}");
        }
    }
}
