﻿using Application.Dtos.SeasonalQuest;
using Application.Interfaces.Common;

namespace Application.Interfaces
{
    public interface ISeasonalQuestService : IBaseQuestService<
        SeasonalQuestDto,
        CreateSeasonalQuestDto,
        UpdateSeasonalQuestDto,
        PatchSeasonalQuestDto>
    {
    }
}
