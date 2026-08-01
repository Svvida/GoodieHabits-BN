using Application.Finance.RecurringTransactions.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Finance.RecurringTransactions.Mappings
{
    public class RecurringTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // LastMaterializedOn is deliberately not exposed — it is generator bookkeeping, not user data.
            config.NewConfig<RecurringTransaction, RecurringTransactionDto>();
        }
    }
}
