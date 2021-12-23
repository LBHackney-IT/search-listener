using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.Logging;
using Hackney.Shared.HousingSearch.Domain.Accounts;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway.Interfaces;
using HousingSearchListener.V1.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway
{
    public class AccountDynamoDbGateway : IAccountDbGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly IAmazonDynamoDB _amazonDynamoDb;

        private readonly ILogger<AccountDynamoDbGateway> _logger;

        public AccountDynamoDbGateway(IDynamoDBContext dynamoDbContext,
            ILogger<AccountDynamoDbGateway> logger,
            IAmazonDynamoDB amazonDynamoDb)
        {
            _logger = logger;
            _amazonDynamoDb = amazonDynamoDb;
            _dynamoDbContext = dynamoDbContext;
        }

        public async Task CreateAccount(Account account)
        {
            await _dynamoDbContext.SaveAsync(EntityFactory.AccountToDatabase(account)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get account record by accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>Account domain model or null</returns>
        [LogCall]
        public async Task<Account> GetByIdAsync(Guid accountId)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for id {accountId}");
            //var result = await _dynamoDbContext.LoadAsync<Hackney.Shared.HousingSearch.Gateways.Entities.Accounts.AccountDbEntity>(accountId).ConfigureAwait(false);
            var result = await _dynamoDbContext.LoadAsync<AccountDbEntity>(accountId).ConfigureAwait(false);
            //var result = await _dynamoDbContext.LoadAsync<Account>(accountId).ConfigureAwait(false);

            return result.ToDomain();
        }

        /// <summary>
        /// Get a list of accounts by parentId
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns>A list of accountdomain models</returns>
        [LogCall]
        public async Task<List<Account>> GetByParentIdAsync(Guid parentId)
        {
            _logger.LogDebug($"Calling IAmazonDynamoDB.QueryAsync to get account list by parentId {parentId}");

            QueryRequest request = new QueryRequest
            {
                TableName = "Accounts",
                IndexName = "parent_account_id_dx",
                KeyConditionExpression = "parent_account_id = :V_parent_account_id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":V_parent_account_id", new AttributeValue { S = parentId.ToString() }}
                },
                ScanIndexForward = true
            };

            var response = await _amazonDynamoDb.QueryAsync(request).ConfigureAwait(false);
            List<Account> data = QueryResponseExtension.ToAccounts(response);

            return data;
        }

        /// <summary>
        /// Update account_balance field for account by accountId
        /// </summary>
        /// <param name="accountId">account id</param>
        /// <param name="balance">New balance value</param>
        /// <returns></returns>
        [LogCall]
        public async Task UpdateAccountBalance(Guid accountId, decimal balance)
        {
            _logger.LogDebug($"Calling IAmazonDynamoDB.UpdateItemAsync for id {accountId} to update total balance to {balance}");

            try
            {
                UpdateItemRequest request = new UpdateItemRequest
                {
                    TableName = "Accounts",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "id", new AttributeValue { S = accountId.ToString() } },
                    },
                    UpdateExpression = "SET consolidated_balance = :V_consolidated_balance",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":V_consolidated_balance", new AttributeValue { N = balance.ToString() } }
                    }
                };

                await _amazonDynamoDb.UpdateItemAsync(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Failed to update account balance for account with id {accountId}. New value: {balance}. Exception message: {ex.Message}. Inner exception message: {ex.InnerException?.Message ?? string.Empty}");

                throw;
            }
        }
    }
}
