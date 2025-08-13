using System.ComponentModel;
using System.Reflection;

namespace ComprasTccApp.Backend.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFriendlyString(this Enum genericEnum)
        {
            return genericEnum
                    .GetType()
                    .GetMember(genericEnum.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description ?? genericEnum.ToString();
        }

        public static T FromString<T>(this string str)
            where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (
                    Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                    is DescriptionAttribute attribute
                )
                {
                    if (attribute.Description == str)
                        return (T)field.GetValue(null);
                }

                if (field.Name == str)
                    return (T)field.GetValue(null);
            }

            throw new ArgumentException(
                $"A string '{str}' não corresponde a nenhum valor do enum {typeof(T).Name}.",
                nameof(str)
            );
        }
    }
}
