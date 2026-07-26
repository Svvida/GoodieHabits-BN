namespace Domain.ValueObjects
{
    /// <summary>
    /// Allow-list of ISO-4217 currency codes the finance module accepts.
    /// Kept intentionally broad; extend as needed. Single currency is stored per user on the profile.
    /// </summary>
    public static class SupportedCurrencies
    {
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "USD", "EUR", "GBP", "PLN", "CHF", "CZK", "SEK", "NOK", "DKK", "JPY",
            "CNY", "AUD", "CAD", "NZD", "UAH", "HUF", "RON", "BGN", "TRY", "INR",
        };

        public static bool IsSupported(string? code)
            => !string.IsNullOrWhiteSpace(code) && All.Contains(code.Trim());
    }
}
