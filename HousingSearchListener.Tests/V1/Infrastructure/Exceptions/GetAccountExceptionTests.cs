using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using System;
using System.Net;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class GetAccountExceptionTests
    {
        [Fact]
        public void GetAccountExceptionConstructorTest()
        {
            var accountId = Guid.NewGuid();
            var statusCode = HttpStatusCode.OK;
            var msg = "Some API error message";

            var ex = new GetAccountException(accountId, statusCode, msg);
            ex.AccountId.Should().Be(accountId);
            ex.StatusCode.Should().Be(statusCode);
            ex.ResponseBody.Should().Be(msg);
            ex.Message.Should().Be($"Failed to get account details for id {accountId}. Status code: {statusCode}; Message: {msg}");
        }
    }
}
