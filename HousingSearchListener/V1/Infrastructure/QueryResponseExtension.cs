using Amazon.DynamoDBv2.Model;
using Hackney.Shared.HousingSearch.Domain.Tenure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.V1.Infrastructure
{
    public class QueryResponseExtension
    {
        public static List<Account> ToAccounts(this QueryResponse response)
        {
            var accounts = new List<Account>();
            foreach (var item in response.Items)
            {
                List<ConsolidatedCharge> consolidatedChargesList = null;

                if (item.Keys.Any(p => p == "consolidated_charges"))
                {
                    var charges = item["consolidated_charges"].L;

                    var chargesItems = charges.Select(p => p.M);
                    consolidatedChargesList = chargesItems.Select(inner =>
                        new ConsolidatedCharge
                        {
                            Amount = decimal.Parse(inner["amount"].N),
                            Frequency = inner["frequency"].S,
                            Type = inner["type"].S
                        }).ToList();
                }

                Tenure tenure = null;
                var tenureDictionary = item["tenure"].M;
                if (item.Keys.Any(p => p == "tenure"))
                {
                    tenure = new Tenure
                    {
                        FullAddress = tenureDictionary["fullAddress"].S,
                        TenureId = tenureDictionary["tenureId"].S,
                        TenureType = new TenureType
                        {
                            Code = tenureDictionary["tenureType"].M["code"].S,
                            Description = tenureDictionary["tenureType"].M["description"].S
                        }
                    };
                }

                accounts.Add(new Account
                {
                    Id = Guid.Parse(item["id"].S),
                    AccountBalance = decimal.Parse(item["account_balance"].N),
                    ConsolidatedCharges = consolidatedChargesList,
                    Tenure = tenure,
                    TargetType = Enum.Parse<TargetType>(item["target_type"].S),
                    TargetId = Guid.Parse(item["target_id"].S),
                    AccountType = Enum.Parse<AccountType>(item["account_type"].S),
                    RentGroupType = Enum.Parse<RentGroupType>(item["rent_group_type"].S),
                    AgreementType = item["agreement_type"].S,
                    CreatedBy = item["created_by"].S,
                    LastUpdatedBy = item["last_updated_by"].S,
                    CreatedAt = DateTime.Parse(item["created_at"].S),
                    LastUpdatedAt = DateTime.Parse(item["last_updated_at"].S),
                    StartDate = DateTime.Parse(item["start_date"].S),
                    EndDate = DateTime.Parse(item["end_date"].S),
                    AccountStatus = Enum.Parse<AccountStatus>(item["account_status"].S),
                    PaymentReference = item["payment_reference"].S,
                    ParentAccountId = Guid.Parse(item["parent_account_id"].S)
                });
            }

            return accounts;
        }
    }
}
