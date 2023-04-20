using System.ComponentModel;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum Calculations
{
    [Description("Time Frequency Analysis")]
    TF,
    [Description("Z-Scored Time Frequency Analysis")]
    NormalizedTF,
    [Description("Calculation unknown")]
    Unknown
};
