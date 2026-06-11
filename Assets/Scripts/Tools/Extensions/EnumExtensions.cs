using UnityEngine;
using System;
using System.Globalization;
using System.ComponentModel;
using System.Linq;

public static class EnumExtensions
{
    public static string GetDescription<T>(this T e) where T : IConvertible
    {
        if (e is Enum)
        {
            Type type = e.GetType();
            Array values = System.Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    var memInfo = type.GetMember(type.GetEnumName(val));
                    var descriptionAttribute = memInfo[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;

                    if (descriptionAttribute != null)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Convert a string value to it's underlying enum value if the description string is
    /// a match to the one defined in the enum, otherwise it returns the default value of the
    /// enum.
    /// From https://stackoverflow.com/questions/4367723/get-enum-from-description-attribute
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="description">String description of the enum</param>
    /// <returns>Correponding enum value</returns>
    public static T GetValueFromDescription<T>(string description)
    {
        var type = typeof(T);
        if (!type.IsEnum) throw new InvalidOperationException();
        foreach (var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute != null)
            {
                if (attribute.Description == description)
                    return (T)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (T)field.GetValue(null);
            }
        }
        return default;
    }
}