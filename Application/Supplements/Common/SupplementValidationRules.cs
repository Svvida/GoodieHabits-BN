namespace Application.Supplements.Common
{
    /// <summary>
    /// Shared field rules for the supplements slices, mirroring <c>FinanceValidationRules</c>.
    /// </summary>
    public static class SupplementValidationRules
    {
        public static bool BeValidHexColor(string? color)
            => color is not null && color.Length == 7 && color[0] == '#';
    }
}
