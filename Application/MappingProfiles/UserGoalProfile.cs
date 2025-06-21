using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles
{
    public class UserGoalProfile : Profile
    {
        public UserGoalProfile()
        {
            CreateMap<UserGoal, UserGoal>();
        }
    }
}
