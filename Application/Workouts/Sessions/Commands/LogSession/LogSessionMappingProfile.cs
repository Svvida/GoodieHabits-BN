using Application.Workouts.Sessions.Dtos;
using Mapster;

namespace Application.Workouts.Sessions.Commands.LogSession
{
    public class LogSessionMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // SessionId and UserProfileId are attached in the controller (route + identity).
            config.NewConfig<LogSessionRequest, LogSessionCommand>()
                .Map(dest => dest.Exercises, src => src.Exercises ?? new List<SessionExerciseInput>());
        }
    }
}
