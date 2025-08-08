using Application.Quests.Dtos;
using Domain.Models;

namespace Application.Common.Interfaces.Quests
{
    public interface IQuestMapper
    {
        QuestDetailsDto MapToDto(Quest quests);
    }
}
