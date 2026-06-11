using BrainTV.Tools.NumberExtensions;
using Newtonsoft.Json;
using System;
using UnityEngine;

/// <summary>
/// Json converter capable of Handling UnityEngine.Color struct
/// </summary>
public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Vector3));
    }

    /// <summary>
    /// Write a Vector3 to a text string 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Vector3)value).ToString());
    }

    /// <summary>
    /// Create a Vector3 from a text string
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Expected format: "(1.00, 2.00, 3.00)" (Vector3.ToString())
        string[] splitedString = ((string)reader.Value).Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        if (splitedString.Length == 0) return Vector3.zero;

        string[] components = splitedString[splitedString.Length - 1].Split(',');
        if (components.Length != 3) return Vector3.zero;

        NumberExtensions.TryParseFloat(components[0], out float x);
        NumberExtensions.TryParseFloat(components[1], out float y);
        NumberExtensions.TryParseFloat(components[2], out float z);

        return new Vector3(x, y, z);
    }
}