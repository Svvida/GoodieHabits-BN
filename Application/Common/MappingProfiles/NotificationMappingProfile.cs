using Application.Common.Dtos;
using Domain.Models;
using Mapster;

namespace Application.Common.MappingProfiles
{
    public class NotificationMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Notification, NotificationDto>()
                .Map(dest => dest.Type, src => src.Type.ToString())
                .Map(dest => dest.Data, src => src.PayloadJson);
        }
    }
}
