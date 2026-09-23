using UnityEngine;

/// <summary>
/// Electrode colour for a correlation value, pulled out of BrainWarden (review M-7): white to
/// red for positive values, white to blue for negative ones. Behaviour is unchanged, including
/// two quirks the tests pin: exactly 0 is green, and |value| > 1 is not clamped.
/// </summary>
public static class CorrelationColor
{
    public static Color For(float value)
    {
        if (value > 0)
        {
            float r = Color.white.r * (1 - value) + Color.red.r * value;
            float g = Color.white.g * (1 - value) + Color.red.g * value;
            float b = Color.white.b * (1 - value) + Color.red.b * value;
            return new Color(r, g, b, 1);
        }
        else if (value < 0)
        {
            float absVal = Mathf.Abs(value);
            float r = Color.white.r * (1 - absVal) + Color.blue.r * absVal;
            float g = Color.white.g * (1 - absVal) + Color.blue.g * absVal;
            float b = Color.white.b * (1 - absVal) + Color.blue.b * absVal;
            return new Color(r, g, b, 1);
        }
        else
        {
            return Color.green;
        }
    }
}
