using AutoMapper;

namespace Application.Quests.UpdateQuestCompletion
{
    public class UpdateQuestCompletionMappingProfile : Profile
    {
        public UpdateQuestCompletionMappingProfile()
        {
            CreateMap<UpdateQuestCompletionRequest, UpdateQuestCompletionCommand>();
        }
    }
}
