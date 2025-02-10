using Application.Dtos.OneTimeQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface IOneTimeQuestService : IBaseQuestService<
        GetOneTimeQuestDto,
        CreateOneTimeQuestDto,
        UpdateOneTimeQuestDto,
        PatchOneTimeQuestDto>
    {
    }
}
