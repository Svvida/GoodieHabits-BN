﻿namespace Application.Dtos.Quests.MonthlyQuest
{
    public class CreateMonthlyQuestDto : BaseCreateQuestDto
    {
        public int StartDay { get; set; }
        public int EndDay { get; set; }
    }
}
