using Application.Dtos.Quests;
using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestMappingService
    {
        BaseGetQuestDto MapToDto(Quest quests);
    }
}
