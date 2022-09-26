using FluentAssertions;
using HousingSearchListener.V1.Infrastructure.Exceptions;
using System;
using Xunit;

namespace HousingSearchListener.Tests.V1.Infrastructure.Exceptions
{
    public class EntityNotIndexedExceptionTests
    {
        [Fact]
        public void EntityNotIndexedExceptionConstructorTest()
        {
            var id = Guid.NewGuid().ToString();

            var ex = new EntityNotIndexedException<SomeEntity>(id);
            ex.Id.Should().Be(id);

            var typeName = typeof(SomeEntity).Name;
            ex.EntityName.Should().Be(typeName);
            ex.Message.Should().Be($"{typeName} with id {id} not indexed in ElasticSearch.");
        }
    }
}
