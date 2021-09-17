using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using System;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class AssetNotIndexedExceptionTests
    {
        [Fact]
        public void AssetNotIndexedExceptionTest()
        {
            var id = Guid.NewGuid().ToString();

            var ex = new AssetNotIndexedException(id);
            ex.Id.Should().Be(id);
            ex.Message.Should().Be($"Asset with id {id} is not indexed in elastic search");
        }
    }
}
