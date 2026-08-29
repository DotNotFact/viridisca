using System.ComponentModel;

namespace ViridiscaUi.Infrastructure.Extensions;

public static class DisplayTypeExtensions
{ 
    public static string GetDisplayName<T>(this T enumType) where T : Enum
    {
        var fieldInfo = enumType.GetType().GetField(enumType.ToString());

        if (fieldInfo is null)
            return enumType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : enumType.ToString();
    }
}