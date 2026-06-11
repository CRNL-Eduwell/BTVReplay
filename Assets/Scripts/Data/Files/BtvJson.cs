using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Shared Json.NET settings for BTVReplay data files.
///
/// The data files are polymorphic on <see cref="IEegFileInfo"/>, which requires type info in
/// the JSON. We keep TypeNameHandling.Auto but pair it with an allow-list
/// <see cref="ISerializationBinder"/>: this still loads existing files (which embed
/// "&lt;Type&gt;, Assembly-CSharp") yet refuses to instantiate any type that is not one of the
/// known data types - closing the well-known TypeNameHandling remote-code-execution gadget
/// vector on a maliciously crafted .dbtv2.
/// </summary>
public static class BtvJson
{
    private static readonly ISerializationBinder s_Binder = new BtvSerializationBinder();

    public static JsonSerializerSettings ReadSettings => new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        SerializationBinder = s_Binder,
    };

    public static JsonSerializerSettings WriteSettings => new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        SerializationBinder = s_Binder,
    };

    private sealed class BtvSerializationBinder : ISerializationBinder
    {
        // The only polymorphic types in BTVReplay data files. Keyed by simple name so files
        // written by any namespace/assembly variant still resolve.
        private static readonly Dictionary<string, Type> s_Allowed = new Dictionary<string, Type>
        {
            { nameof(MicromedFileInfo), typeof(MicromedFileInfo) },
            { nameof(ElanFileInfo), typeof(ElanFileInfo) },
            { nameof(BrainvisionFileInfo), typeof(BrainvisionFileInfo) },
            { nameof(EdfFileInfo), typeof(EdfFileInfo) },
        };

        public Type BindToType(string assemblyName, string typeName)
        {
            string simple = typeName.Contains(".") ? typeName.Substring(typeName.LastIndexOf('.') + 1) : typeName;
            if (s_Allowed.TryGetValue(simple, out Type type)) return type;
            throw new JsonSerializationException("Type '" + typeName + "' is not an allowed BTVReplay data type.");
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            // Preserve the existing on-disk format ("<Type>, Assembly-CSharp").
            assemblyName = "Assembly-CSharp";
            typeName = serializedType.Name;
        }
    }
}
