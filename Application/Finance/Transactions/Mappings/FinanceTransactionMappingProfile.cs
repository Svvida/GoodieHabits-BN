using Application.Finance.Transactions.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Finance.Transactions.Mappings
{
    public class FinanceTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // NetAmount comes straight off the computed property, and Corrections maps by name. The recursion is
            // bounded by the domain: a correction can never have corrections of its own.
            config.NewConfig<FinanceTransaction, TransactionDto>();
        }
    }
}
