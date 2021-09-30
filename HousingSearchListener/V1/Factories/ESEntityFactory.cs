using HousingSearchListener.V1.Domain.ElasticSearch.Person;
using HousingSearchListener.V1.Domain.ElasticSearch.Tenure;
using HousingSearchListener.V1.Domain.Person;
using HousingSearchListener.V1.Domain.Tenure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HousingSearchListener.V1.Factories
{
    public class ESEntityFactory : IESEntityFactory
    {
        private List<QueryablePersonTenure> CreatePersonTenures(List<Tenure> tenures)
        {
            return tenures.Select(x => new QueryablePersonTenure
            {
                AssetFullAddress = x.AssetFullAddress,
                EndDate = x.EndDate,
                Id = x.Id,
                StartDate = x.StartDate,
                Type = x.Type
            }).ToList();
        }

        public QueryablePerson CreatePerson(Person person)
        {
            return new QueryablePerson
            {
                Id = person.Id,
                DateOfBirth = person.DateOfBirth,
                PlaceOfBirth = person.PlaceOfBirth,
                Title = person.Title,
                Firstname = person.FirstName,
                Surname = person.Surname,
                MiddleName = person.MiddleName,
                PreferredFirstname = person.PreferredFirstName,
                PreferredSurname = person.PreferredSurname,
                PersonTypes = person.PersonTypes,
                Tenures = person.Tenures != null ? CreatePersonTenures(person.Tenures) : new List<QueryablePersonTenure>()
            };
        }

        public QueryableTenure CreateQueryableTenure(TenureInformation tenure)
        {
            return new QueryableTenure
            {
                Id = tenure.Id,
                StartOfTenureDate = tenure.StartOfTenureDate,
                EndOfTenureDate = tenure.EndOfTenureDate,
                TenureType = new QueryableTenureType()
                {
                    Code = tenure.TenureType.Code,
                    Description = tenure.TenureType.Description
                },
                PaymentReference = tenure.PaymentReference,
                HouseholdMembers = CreateQueryableHouseholdMembers(tenure.HouseholdMembers),
                TenuredAsset = new QueryableTenuredAsset()
                {
                    FullAddress = tenure.TenuredAsset?.FullAddress,
                    Id = tenure.TenuredAsset?.Id,
                    Type = tenure.TenuredAsset?.Type,
                    Uprn = tenure.TenuredAsset?.Uprn
                }
            };
        }

        public List<QueryableHouseholdMember> CreateQueryableHouseholdMembers(List<HouseholdMembers> householdMembers)
        {
            if (householdMembers is null) return new List<QueryableHouseholdMember>();

            return householdMembers.Select(x => new QueryableHouseholdMember()
            {
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                Id = x.Id,
                IsResponsible = x.IsResponsible,
                PersonTenureType = x.PersonTenureType,
                Type = x.Type
            }).ToList();
        }

        public Domain.ElasticSearch.Asset.QueryableTenure CreateAssetQueryableTenure(TenureInformation tenure)
        {
            if (tenure is null) throw new ArgumentNullException(nameof(tenure));

            return new Domain.ElasticSearch.Asset.QueryableTenure()
            {
                Id = tenure.Id,
                EndOfTenureDate = tenure.EndOfTenureDate,
                PaymentReference = tenure.PaymentReference,
                StartOfTenureDate = tenure.StartOfTenureDate,
                TenuredAsset = new Domain.ElasticSearch.Asset.QueryableTenuredAsset()
                {
                    FullAddress = tenure.TenuredAsset.FullAddress,
                    Id = tenure.TenuredAsset.Id,
                    Type = tenure.TenuredAsset.Type,
                    Uprn = tenure.TenuredAsset.Uprn,
                },
                Type = tenure.TenureType.Description
            };
        }
    }
}