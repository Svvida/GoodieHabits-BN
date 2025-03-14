﻿namespace Application.Dtos.Quests.SeasonalQuest
{
    public class UpdateSeasonalQuestDto : BaseUpdateQuestDto
    {
        public string Season { get; set; } = string.Empty; // Prevents ASP.NET Core default validation errors.
    }
}
