using System;
using System.Net;

namespace HousingSearchListener.V1.Infrastructure.Exceptions
{
    public class GetAccountException : Exception
    {
        public Guid AccountId { get; }
        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }

        public GetAccountException(Guid id, HttpStatusCode statusCode, string responseBody)
            : base($"Failed to get account details for id {id}. Status code: {statusCode}; Message: {responseBody}")
        {
            AccountId = id;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
