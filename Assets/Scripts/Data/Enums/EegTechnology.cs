using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum EegTechnology
{
    [Description("Intracranial EEG")]
    Intra,
    [Description("Scalp EEG")]
    Scalp,
    [Description("Error Eeg Technology")]
    Unknown
};
