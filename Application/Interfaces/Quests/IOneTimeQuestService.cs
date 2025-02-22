using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces.Quests
{
    public interface IOneTimeQuestService : IBaseQuestService<
        GetOneTimeQuestDto,
        CreateOneTimeQuestDto,
        UpdateOneTimeQuestDto,
        PatchOneTimeQuestDto>
    {
    }
}
