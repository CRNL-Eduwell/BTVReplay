using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum ShortcutActions
{
    //[Description("Something")]
    None,
    Focus,
    Move,
    ChangeOption,
    UpdateOptionValue
}

public enum ShortcutActionsParameters
{
    //[Description("Something")]
    None,
    In,
    Out,
    Left,
    Right,
    Up,
    Down
}
