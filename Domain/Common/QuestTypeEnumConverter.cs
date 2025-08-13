using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Common
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
                // Try parsing by EnumMember attribute value
                foreach (var field in typeof(QuestTypeEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var enumMemberAttribute = field.GetCustomAttribute<EnumMemberAttribute>();
                    if (enumMemberAttribute != null && string.Equals(enumMemberAttribute.Value, stringValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return Enum.Parse(typeof(QuestTypeEnum), field.Name);
                    }
                }

                if (Enum.TryParse<QuestTypeEnum>(stringValue, true, out var result))
                    return result;

                throw new InvalidArgumentException($"'{stringValue}' is not a valid QuestTypeEnum.");
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
