using BrainTV.Tools.NumberExtensions;
using Newtonsoft.Json;
using System;
using UnityEngine;

/// <summary>
/// Json converter capable of Handling UnityEngine.Color struct
/// </summary>
public class Vector2Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Vector2));
    }

    /// <summary>
    /// Write a Vector2 to a text string 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Vector2)value).ToString());
    }

    /// <summary>
    /// Create a Vector2 from a text string
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Expected format: "(1.00, 2.00)" (Vector2.ToString())
        string[] splitedString = ((string)reader.Value).Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        if (splitedString.Length == 0) return Vector2.zero;

        string[] components = splitedString[splitedString.Length - 1].Split(',');
        if (components.Length != 2) return Vector2.zero;

        NumberExtensions.TryParseFloat(components[0], out float x);
        NumberExtensions.TryParseFloat(components[1], out float y);

        return new Vector2(x, y);
    }
}