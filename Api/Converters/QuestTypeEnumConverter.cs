using System.ComponentModel;
using System.Globalization;
using Domain.Enum;
using Domain.Exceptions;

namespace Api.Converters
{
    public class QuestTypeEnumConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                if (Enum.TryParse<QuestTypeEnum>(stringValue, true, out var result))
                    return result;

                throw new InvalidArgumentException($"'{stringValue}' is not a valid QuestTypeEnum.");
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
