using AutoMapper;

namespace Application.UserGoals.CreateUserGoal
{
    public class CreateUserGoalMappingProfile : Profile
    {
        public CreateUserGoalMappingProfile()
        {
            CreateMap<CreateUserGoalRequest, CreateUserGoalCommand>();
        }
    }
}
