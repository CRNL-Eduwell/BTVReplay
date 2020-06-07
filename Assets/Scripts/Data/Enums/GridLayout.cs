using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum GridLayout
{
    [Description("2 by 2 Grid")]
    TwoBy2,
    [Description("2 by 3 Grid")]
    TwoBy3,
    [Description("1 by 3 Grid")]
    OneBy3
};
