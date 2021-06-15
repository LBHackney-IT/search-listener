using System;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.SQSEvents;
using FluentAssertions;
using HousingSearchListener.Gateways;
using Xunit;

namespace HousingSearchListener.Tests.Gateways
{
    public class PersonMessageFactoryTests
    {
        private PersonMessageFactory _sut;

        public PersonMessageFactoryTests()
        {
            _sut = new PersonMessageFactory();
        }

        [Fact]
        public void GivenAnSqsRecord_WhenProcessed_ShouldDeserializeCorrectly()
        {
            // given
            SetupEnvVariables();
            var message =
                "{ \"id\": \"8e648f3d-9556-4896-8400-211cb1c5451b\", \"eventType\": \"PersonCreatedEvent\", \"sourceDomain\": \"Person\", \"sourceSystem\": " +
                "\"PersonAPI\", \"version\": \"v1\", \"correlationId\": \"f4d541d0-7c07-4524-8296-2d0d50cb58f4\", \"dateTime\": \"2021-05-17T11:59:57.25Z\", " +
                "\"user\": { \"id\": \"ac703d87-c100-40ec-90a0-dabf183e7377\", \"name\": \"Joe Bloggs\", \"email\": \"joe.bloggs@hackney.gov.uk\" }, " +
                "\"entityId\": \"45c76564-2e38-48f3-bb31-6bab2fef8623\" }";

            // when
            var result = _sut.Create(new SQSEvent.SQSMessage
            {
                Body = message
            });

            // then
            result.Should().NotBeNull();
        }

        private static void SetupEnvVariables()
        {
            Environment.SetEnvironmentVariable("PersonApiUrl", "http://127.0.0.1");
            Environment.SetEnvironmentVariable("PersonApiToken",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEwNSIsImNvbnN1bWVyTmFtZSI6IlRlbmFudHMgYW5kIExlYXNlaG9sZGVycyIsImNvbnN1bWVyVHlwZSI6IjIiLCJuYmYiOjE2MjEzMzAxODcsImV4cCI6MTkzNjg2Mjk4NywiaWF0IjoxNjIxMzMwMTg3fQ.cK37Miiu3huq_lHHn0kyfdOvAqXCO0cyXquiNOiK318");
        }
    }
}
