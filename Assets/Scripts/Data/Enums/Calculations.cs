using System.ComponentModel;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum Calculations
{
    [Description("Time Frequency Analysis")]
    TF,
    [Description("Calculation unknown")]
    Unknown
};
