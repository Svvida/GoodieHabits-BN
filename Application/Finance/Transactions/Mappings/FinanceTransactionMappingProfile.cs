using Application.Finance.Transactions.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Finance.Transactions.Mappings
{
    public class FinanceTransactionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<FinanceTransaction, TransactionDto>();
        }
    }
}
