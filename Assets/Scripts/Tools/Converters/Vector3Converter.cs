using BrainTV.Tools.NumberExtensions;
using Newtonsoft.Json;
using System;
using UnityEngine;

/// <summary>
/// Json converter capable of Handling UnityEngine.Color struct
/// </summary>
class Vector3Converter : JsonConverter
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
        string[] splitedString = ((string)reader.Value).Split(new char[] { '(', ')', ','}, StringSplitOptions.None);

        NumberExtensions.TryParseFloat(splitedString[0], out float x);
        NumberExtensions.TryParseFloat(splitedString[1], out float y);
        NumberExtensions.TryParseFloat(splitedString[2], out float z);

        return splitedString.Length == 3 ? new Vector3(x, y, z) : new Vector3(0, 0, 0);
    }
}