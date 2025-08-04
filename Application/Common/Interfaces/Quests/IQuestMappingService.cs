using Application.Quests.Dtos;
using Domain.Models;

namespace Application.Common.Interfaces.Quests
{
    public interface IQuestMappingService
    {
        QuestDetailsDto MapToDto(Quest quests);
    }
}
