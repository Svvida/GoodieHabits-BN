using Mapster;

namespace Application.UserGoals.CreateUserGoal
{
    public class CreateUserGoalMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateUserGoalRequest, CreateUserGoalCommand>();
        }
    }
}
