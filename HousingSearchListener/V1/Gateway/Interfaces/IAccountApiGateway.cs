﻿using HousingSearchListener.V1.Boundary.Response;
using System;
using System.Threading.Tasks;

namespace HousingSearchListener.V1.Gateway.Interfaces
{
    public interface IAccountApiGateway
    {
        Task<AccountResponse> GetAccountByIdAsync(Guid id, Guid correlationId);
    }
}
