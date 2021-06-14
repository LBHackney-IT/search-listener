using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Xunit;

namespace HousingSearchListener.Tests
{
    public class HousingSearchListenerTests
    {
        private HousingSearchListener _sut;

        public HousingSearchListenerTests()
        {
            _sut = new HousingSearchListener();
        }

        [Fact]
        public async Task TestingUrlGeneration()
        {
            Environment.SetEnvironmentVariable("PersonApiUrl", "http://127.0.0.1");
            Environment.SetEnvironmentVariable("PersonApiToken", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEwNSIsImNvbnN1bWVyTmFtZSI6IlRlbmFudHMgYW5kIExlYXNlaG9sZGVycyIsImNvbnN1bWVyVHlwZSI6IjIiLCJuYmYiOjE2MjEzMzAxODcsImV4cCI6MTkzNjg2Mjk4NywiaWF0IjoxNjIxMzMwMTg3fQ.cK37Miiu3huq_lHHn0kyfdOvAqXCO0cyXquiNOiK318");

            var message =
                "{ \"id\": \"8e648f3d-9556-4896-8400-211cb1c5451b\", \"eventType\": \"PersonCreatedEvent\", \"sourceDomain\": \"Person\", \"sourceSystem\": " +
                "\"PersonAPI\", \"version\": \"v1\", \"correlationId\": \"f4d541d0-7c07-4524-8296-2d0d50cb58f4\", \"dateTime\": \"2021-05-17T11:59:57.25Z\", " +
                "\"user\": { \"id\": \"ac703d87-c100-40ec-90a0-dabf183e7377\", \"name\": \"Joe Bloggs\", \"email\": \"joe.bloggs@hackney.gov.uk\" }, " +
                "\"entityId\": \"45c76564-2e38-48f3-bb31-6bab2fef8623\" }";

            await _sut.FunctionHandler(new SNSEvent
            {
                Records = new List<SNSEvent.SNSRecord>(){
                new SNSEvent.SNSRecord
                {
                    Sns = new SNSEvent.SNSMessage
                    {
                        Message = message
                    }
                }
            }
            });
        }
    }
}
