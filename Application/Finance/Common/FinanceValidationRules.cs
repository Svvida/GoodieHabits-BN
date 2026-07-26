namespace Application.Finance.Common
{
    /// <summary>Shared FluentValidation predicates for the finance module.</summary>
    public static class FinanceValidationRules
    {
        public static bool BeValidHexColor(string? color)
            => color is not null && color.Length == 7 && color[0] == '#';
    }
}
