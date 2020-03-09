using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MeshConfiguration
{
    [Description("Left/Right Mesh")]
    LeftRight,
    [Description("Single Mesh")]
    Single,
    [Description("Error Mesh Configuration")]
    Unknown
}
