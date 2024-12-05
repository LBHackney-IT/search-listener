﻿using Hackney.Core.DynamoDb;
using Hackney.Shared.HousingSearch.Domain.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IContractApiGateway
    {
        Task<Contract> GetContractByIdAsync(Guid entityId, Guid correlationId);
        Task<List<Contract>> GetContractsByAssetIdAsync(Guid targetId, Guid correlationId);

    }
}