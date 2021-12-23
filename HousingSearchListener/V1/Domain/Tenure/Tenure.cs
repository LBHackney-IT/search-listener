using Hackney.Shared.HousingSearch.Domain.Accounts;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HousingSearchListener.V1.Domain.Tenure
{
    // ToDo: delete this class
    public class Tenure
    {
        /// <example>
        ///     31245
        /// </example>
        [NotNull]
        public string TenureId { get; set; }

        /// <example>
        ///     Introductory
        /// </example>
        [NotNull]
        public TenureType TenureType { get; set; }

        /// <example>
        ///     285 Avenue, 315 Amsterdam
        /// </example>
        [NotNull]
        public string FullAddress { get; set; }

        public IEnumerable<PrimaryTenant> PrimaryTenants { get; set; }
    }
}
