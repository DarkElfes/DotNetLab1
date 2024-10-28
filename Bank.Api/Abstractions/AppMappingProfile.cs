using AutoMapper;
using Bank.Api.Accounts;
using Bank.Api.Cards;
using Bank.Api.Transactions.TransactionModels;
using Bank.Shared.Accounts;
using Bank.Shared.Cards;
using Bank.Shared.Transactions.TransactionsDto;

namespace Bank.Api.Abstractions;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<Card, CardDto>();

        CreateMap<TellerMachineTransaction, TellerMachineTransactionDto>(); 
        CreateMap<ServicesPaymentTransaction, ServicesPaymentTransactionDto>();
        CreateMap<TransferTransaction, TransferTransactionDto>()
            .ForMember(dest => dest.SenderCardId, opt => opt.MapFrom(src => src.SenderCard.Id))
            .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.SenderCard.Owner.Username))
            .ForMember(dest => dest.ReceiverCardId, opt => opt.MapFrom(src => src.ReceiverCard.Id))
            .ForMember(dest => dest.ReceiverUsername, opt => opt.MapFrom(src => src.ReceiverCard.Owner.Username));


    }

}
