using Application.Quests.Dtos;
using Domain.Models;

namespace Application.Quests
{
    public interface IQuestMapper
    {
        QuestDetailsDto MapToDto(Quest quests);
    }
}
