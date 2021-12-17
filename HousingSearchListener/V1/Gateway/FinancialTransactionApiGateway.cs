using System;
using System.Threading.Tasks;
using Hackney.Core.Logging;
using HousingSearchListener.V1.Domain.Transaction;

namespace HousingSearchListener.V1.Gateway
{
    public class FinancialTransactionApiGateway : IFinancialTransactionApiGateway
    {
        private const string ApiName = "FinancialTransaction";
        private const string FinancialTransactionApiUrl = "FinancialTransactionApiUrl";
        private const string FinancialTransactionApiToken = "FinancialTransactionApiToken";

        private readonly IApiGateway _apiGateway;

        public FinancialTransactionApiGateway(IApiGateway apiGateway)
        {
            _apiGateway = apiGateway;
            _apiGateway.Initialise(ApiName, FinancialTransactionApiUrl, FinancialTransactionApiToken);
        }
        [LogCall]
        public async Task<TransactionResponseObject> GetTransactionByIdAsync(Guid id, Guid targetId, Guid correlationId)
        {
            var route = $"{_apiGateway.ApiRoute}/transactions/{id}?targetId={targetId}";
            return await _apiGateway.GetByIdAsync<TransactionResponseObject>(route, id, correlationId).ConfigureAwait(false);
        }
    }
}
