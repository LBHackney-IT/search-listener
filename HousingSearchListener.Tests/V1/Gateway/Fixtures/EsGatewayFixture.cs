﻿using AutoFixture;
using HousingSearchListener.V1.Domain.Account;
using HousingSearchListener.V1.Domain.ElasticSearch;
using HousingSearchListener.V1.Gateway;
using System;
using System.Collections.Generic;

namespace HousingSearchListener.Tests.V1.Gateway.Fixtures
{
    public class EsGatewayFixture
    {
        protected readonly List<Action> _cleanup = new List<Action>();
        protected readonly Fixture _fixture;

        public static ESPerson EsPerson { get; private set; }
        public static QueryableTenure QueryableTenure { get; private set; }
        public static Account PersonAccount { get; private set; }
        public static ESPersonTenure EsPersonTenure { get; private set; }

        public EsGatewayFixture() 
        {
            _fixture = new Fixture();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }

        public void GivenTheEsPersonDoesNotExist()
        {
            // nothing to do here
        }

        public ESPerson GivenTheEsPersonExists()
        {
            var tenures = new List<ESPersonTenure> { GivenTheEsPersonTenureExists() };

            EsPerson = _fixture.Build<ESPerson>()
                           .With(x => x.DateOfBirth, DateTime.UtcNow.AddYears(-30).ToString())
                           .With(x => x.Tenures, tenures)
                           .Create();

            return EsPerson;
        }

        public void GivenTheQueryableTenureDoesNotExist()
        {
            // nothing to do here
        }

        public QueryableTenure GivenTheQueryableTenureExists()
        {
            QueryableTenure = _fixture.Build<QueryableTenure>()
                           .With(x => x.StartOfTenureDate, DateTime.UtcNow.AddMonths(-10).ToString())
                           .With(x => x.EndOfTenureDate, (string)null)
                           .Create();

            return QueryableTenure;
        }

        public void GivenTheAccountDoesNotExist()
        {
            // nothing to do here
        }

        public Account GivenTheAccountExists()
        {
            PersonAccount = _fixture.Build<Account>()
                           .With(x => x.AccountBalance, 42M)
                           .Create();

            return PersonAccount;
        }

        public void GivenTheEsPersonTenureDoesNotExists()
        {
            // nothing to do here
        }

        public ESPersonTenure GivenTheEsPersonTenureExists()
        {
            string tenureId = "ff40861c-5543-41fb-9f00-0a9586b5178b";
            EsPersonTenure = new ESPersonTenure
            {
                Id = tenureId,
                Type = "TenureType",
                StartDate = DateTime.Today.ToString(),
                EndDate = DateTime.Today.AddDays(1).ToString(),
                AssetFullAddress = "FullAddress",
                PostCode = "asd",
                PaymentReference = "12345678",
                TotalBalance = 53
            };

            return EsPersonTenure;
        }
    }
}
