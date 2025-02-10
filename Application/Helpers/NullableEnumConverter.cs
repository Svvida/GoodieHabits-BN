using AutoMapper;

namespace Application.Helpers
{
    internal class NullableEnumConverter<TEnum> : IValueConverter<string?, TEnum?> where TEnum : struct, Enum
    {
        public TEnum? Convert(string? source, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            return Enum.TryParse<TEnum>(source, true, out var result) ? result : null;
        }
    }
}
