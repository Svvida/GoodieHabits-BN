using Application.Dtos.Quests.SeasonalQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces.Quests
{
    public interface ISeasonalQuestService : IBaseQuestService<
        GetSeasonalQuestDto,
        CreateSeasonalQuestDto,
        UpdateSeasonalQuestDto,
        SeasonalQuestCompletionPatchDto>
    {
    }
}
