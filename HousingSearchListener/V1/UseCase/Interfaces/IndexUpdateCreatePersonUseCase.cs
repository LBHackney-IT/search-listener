using System.Threading.Tasks;
using HousingSearchListener.V1.Boundary;
using HousingSearchListener.V1.Factories;
using HousingSearchListener.V1.Gateway;

namespace HousingSearchListener.V1.UseCase.Interfaces
{
    public class IndexUpdateCreatePersonUseCase : IIndexUpdatePersonUseCase
    {
        private readonly IEsGateway _esGateway;
        private readonly IPersonApiGateway _personApiGateway;
        private readonly IESEntityFactory _esPersonFactory;
        private readonly IIndexCreatePersonUseCase _createPersonUseCase;

        public IndexUpdateCreatePersonUseCase(IEsGateway esGateway, IPersonApiGateway personApiGateway,
            IESEntityFactory esPersonFactory, IIndexCreatePersonUseCase createPersonUseCase)
        {
            _esGateway = esGateway;
            _personApiGateway = personApiGateway;
            _esPersonFactory = esPersonFactory;
            _createPersonUseCase = createPersonUseCase;
        }

        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            // Same as Create Person
            await _createPersonUseCase.ProcessMessageAsync(message);


        }
    }
}