using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using System;
using System.Net;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class GetPersonExceptionTests
    {
        [Fact]
        public void GetPersonExceptionConstructorTest()
        {
            var personId = Guid.NewGuid();
            var statusCode = HttpStatusCode.OK;
            var msg = "Some API error message";

            var ex = new GetPersonException(personId, statusCode, msg);
            ex.PersonId.Should().Be(personId);
            ex.StatusCode.Should().Be(statusCode);
            ex.ResponseBody.Should().Be(msg);
            ex.Message.Should().Be($"Failed to get person details for id {personId}. Status code: {statusCode}; Message: {msg}");
        }
    }
}
