namespace Application.Common
{
    public static class EnumHelper
    {
        public static TEnum? ParseNullable<TEnum>(string? value) where TEnum : struct
        {
            if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value, true, out var result))
                return result;

            return null;
        }
    }
}
