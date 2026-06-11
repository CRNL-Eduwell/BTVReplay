using BrainTV.Tools.NumberExtensions;
using Newtonsoft.Json;
using System;
using UnityEngine;

/// <summary>
/// Json converter capable of Handling UnityEngine.Color struct
/// </summary>
public class ColorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Color));
    }

    /// <summary>
    /// Write a color to a text string => RGBA(1.000, 0.557, 0.000, 1.000)
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Color)value).ToString());
    }

    /// <summary>
    /// Create a color from a text string => RGBA(1.000, 0.557, 0.000, 1.000)
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string[] splitedString = ((string)reader.Value).Split(new char[] { '(', ')' }, StringSplitOptions.None);
        if (splitedString.Length < 2) return Color.white;

        string[] splitedColors = splitedString[1].Split(new char[] { ',' }, StringSplitOptions.None);
        if (splitedColors.Length != 4) return Color.white;

        NumberExtensions.TryParseFloat(splitedColors[0], out float r);
        NumberExtensions.TryParseFloat(splitedColors[1], out float g);
        NumberExtensions.TryParseFloat(splitedColors[2], out float b);
        NumberExtensions.TryParseFloat(splitedColors[3], out float a);

        return new Color(r, g, b, a);
    }
}