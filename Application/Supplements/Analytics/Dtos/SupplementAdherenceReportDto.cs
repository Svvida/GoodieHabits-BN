namespace Application.Supplements.Analytics.Dtos
{
    /// <summary>How closely one supplement's plan was followed.</summary>
    public class SupplementAdherenceItemDto
    {
        public int SupplementId { get; set; }
        public string SupplementName { get; set; } = string.Empty;

        /// <summary>Planned doses per day — the number of schedule slots.</summary>
        public int SlotsPerDay { get; set; }

        /// <summary>Planned doses whose day has been evaluated (see the report's remarks).</summary>
        public int Scheduled { get; set; }

        /// <summary>Planned doses actually ticked off. Ad-hoc doses are excluded — they were never scheduled.</summary>
        public int Taken { get; set; }

        /// <summary>Percentage, or null when nothing has been evaluated yet. Not clamped at 100.</summary>
        public decimal? Rate { get; set; }
    }

    /// <summary>
    /// Adherence over a date range, per supplement and overall.
    /// <para>
    /// ⚠️ <b>Denominator rule</b>, borrowed verbatim from quest completion rates: a day counts against the user
    /// only once it has <em>fully elapsed</em> in their local calendar, or once something was already taken
    /// that day. Without it, opening the app at 09:00 would report a third of the plan already missed — wrong
    /// at exactly the moment the number is being read. A rate of <c>null</c> means "nothing evaluated yet",
    /// which is a different statement from 0% and should not be coloured like one.
    /// </para>
    /// <para>Only active supplements are reported: an adherence score for a plan you have retired is noise.</para>
    /// </summary>
    public class SupplementAdherenceReportDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int Scheduled { get; set; }
        public int Taken { get; set; }
        public decimal? Rate { get; set; }
        public List<SupplementAdherenceItemDto> Items { get; set; } = [];
    }
}
