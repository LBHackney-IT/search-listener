using Amazon.DynamoDBv2.Model;
using Hackney.Shared.HousingSearch.Domain.Accounts;
using Hackney.Shared.HousingSearch.Domain.Accounts.Enum;
using Hackney.Shared.HousingSearch.Gateways.Entities.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.V1.Infrastructure
{
    public static class QueryResponseExtension
    {
        public static List<Account> ToAccounts(this QueryResponse response)
        {
            List<AccountDbEntity> accounts = new List<AccountDbEntity>();
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                List<ConsolidatedChargeDbEntity> consolidatedChargesList = null;

                if (item.Keys.Any(p => p == "consolidated_charges"))
                {
                    var charges = item["consolidated_charges"].L;

                    var chargesItems = charges.Select(p => p.M);
                    consolidatedChargesList = new List<ConsolidatedChargeDbEntity>();
                    foreach (Dictionary<string, AttributeValue> inneritem in chargesItems)
                    {
                        consolidatedChargesList.Add(new ConsolidatedChargeDbEntity
                        {
                            Amount = decimal.Parse(inneritem["amount"].N),
                            Frequency = inneritem["frequency"].S,
                            Type = inneritem["type"].S
                        });
                    }
                }

                TenureDbEntity tenure = null;
                if (item.Keys.Any(p => p == "tenure"))
                {
                    var _tenure = item["tenure"].M;
                    tenure = new TenureDbEntity();
                    tenure.FullAddress = _tenure["fullAddress"].S;
                    tenure.TenureId = _tenure["tenureId"].S;
                    tenure.TenureType = new TenureTypeDbEntity
                    {
                        Code = _tenure["tenureType"].M["code"].S,
                        Description = _tenure["tenureType"].M["description"].S
                    };
                    if (_tenure.ContainsKey("primaryTenants"))
                    {
                        tenure.PrimaryTenants = new List<PrimaryTenantsDbEntity>();
                        foreach (var primaryItems in _tenure["primaryTenants"].L)
                        {
                            tenure.PrimaryTenants.Add(new PrimaryTenantsDbEntity
                            {
                                FullName = primaryItems.M["fullName"].S,
                                Id = Guid.Parse(primaryItems.M["id"].S)
                            });
                        }
                    }
                }

                accounts.Add(new AccountDbEntity
                {
                    Id = Guid.Parse(item["id"].S),
                    AccountBalance = decimal.Parse(item["account_balance"].N),
                    ConsolidatedCharges = consolidatedChargesList,
                    Tenure = tenure,
                    TargetType = Enum.Parse<TargetType>(item["target_type"].S),
                    TargetId = Guid.Parse(item["target_id"].S),
                    AccountType = Enum.Parse<AccountType>(item["account_type"].S),
                    RentGroupType = Enum.Parse<RentGroupType>(item["rent_group_type"].S),
                    AgreementType = item.ContainsKey("agreement_type") ? item["agreement_type"].S : null,
                    CreatedBy = item.ContainsKey("created_by") ? item["created_by"].S : null,
                    LastUpdatedBy = item.ContainsKey("last_updated_by") ? item["last_updated_by"].S : null,
                    CreatedAt = DateTime.Parse(item["created_at"].S),
                    LastUpdatedAt = DateTime.Parse(item["last_updated_at"].S),
                    StartDate = DateTime.Parse(item["start_date"].S),
                    EndDate = item.ContainsKey("end_date") ? DateTime.Parse(item["end_date"].S) : (DateTime?)null,
                    AccountStatus = Enum.Parse<AccountStatus>(item["account_status"].S),
                    PaymentReference = item["payment_reference"].S,
                    ParentAccountId = Guid.Parse(item["parent_account_id"].S)
                });
            }

            return accounts.Select(a => a.ToAccount()).ToList();
        }
    }
}
