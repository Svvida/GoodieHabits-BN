using Domain.Enums;

namespace Application.Supplements.Catalog.Dtos
{
    /// <summary>
    /// A supplement in the user's catalog, with its schedule. The checklist is built from these slots; the
    /// catalog itself is also what the client offers when logging an unplanned dose.
    /// </summary>
    public class SupplementDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SupplementUnitEnum Unit { get; set; }

        /// <summary>Fallback dose for an ad-hoc intake that names no slot.</summary>
        public decimal? DefaultAmount { get; set; }

        public string? Note { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }

        /// <summary>Inactive supplements drop out of the checklist but keep every dose ever logged.</summary>
        public bool IsActive { get; set; }

        public List<SupplementSlotDto> Slots { get; set; } = [];
    }
}
