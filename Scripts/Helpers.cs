using Godot;
using Godot.NativeInterop;

public enum BlendModes
{
    BLEND,
    MULTIPLY,
    SCREEN,
    OVERLAY
}

/// <summary>
/// Special thanks to Prof. Aanjaneya <3
/// </summary>
public static class ColorOperations
{
    public static Color Mix(Color top, Color bottom, BlendModes mode)
    {
        switch (mode)
        {
            case BlendModes.MULTIPLY:
                
                return Multiply(top, bottom);

            case BlendModes.SCREEN:
                
                return Screen(top, bottom);

            case BlendModes.OVERLAY:

                return Overlay(top, bottom);

            case BlendModes.BLEND:
            default:

                return Blend(top, bottom);
        }
    }

    public static Color Blend(Color top, Color bottom)
    {
        // Prevent division by 0; they're both completely transparent anyways
        if (top.A == 0 && bottom.A == 0) return new Color(0, 0, 0, 0);

        float newA = top.A + ((1 - top.A) * bottom.A);

        float Formula(float a, float b)
        {
            return ((top.A * a) + (1 - top.A) * b * bottom.A) / newA;
        }

        float newR = Formula(top.R, bottom.R);
        float newG = Formula(top.G, bottom.G);
        float newB = Formula(top.B, bottom.B);

        return new Color(newR, newG, newB, newA); 
    }

    public static Color Multiply(Color top, Color bottom)
    {
        float newR = top.R * bottom.R;
        float newG = top.G * bottom.G;
        float newB = top.B * bottom.B;
        
        return Blend(new Color(newR, newG, newB, top.A), bottom);
    }

    public static Color Screen(Color top, Color bottom)
    {
        float Formula(float a, float b)
        {
            return 1 - (1 - a) * (1 - b);
        }

        float newR = Formula(top.R, bottom.R);
        float newG = Formula(top.G, bottom.G);
        float newB = Formula(top.B, bottom.B);

        return Blend(new Color(newR, newG, newB, top.A), bottom);
    }

    public static Color Overlay(Color top, Color bottom)
    {
        float Formula(float a, float b)
        {
            return (a < 0.5) ? (2 * a * b) : (1 - 2 * (1 - a) * (1 - b));
        }

        float newR = Formula(top.R, bottom.R);
        float newG = Formula(top.G, bottom.G);
        float newB = Formula(top.B, bottom.B);

        return Blend(new Color(newR, newG, newB, top.A), bottom);
    }

    /// <summary>
    /// Adjusts the hue of a color's HSV representation, and returns the adjusted color.
    /// If the adjustment would push the hue past 0 or 1, it wraps around to the other end of the range.
    /// </summary>
    /// <param name="color">The color to adjust.</param>
    /// <param name="amount">The amount to adjust by.</param>
    /// <returns>The color with the same saturation and value and an adjusted hue.</returns>
    public static Color AdjustHue(Color color, float amount)
    {
        color.ToHsv(out float h, out float s, out float v);

        float newH = (h + amount) % 1;

        return Color.FromHsv(newH, s, v);
    }

    /// <summary>
    /// Adjusts the saturation of a color's HSV representation, and returns the adjusted color.
    /// If the adjustment would push the saturation past 0 or 1, it clamps to 0 or 1, respectively.
    /// </summary>
    /// <param name="color">The color to adjust.</param>
    /// <param name="amount">The amount to adjust by.</param>
    /// The color with the same hue and value and an adjusted saturation.
    /// </returns>
    public static Color AdjustSaturation(Color color, float amount)
    {
        color.ToHsv(out float h, out float s, out float v);

        float newS = Mathf.Clamp(s + amount, 0, 1);

        return Color.FromHsv(h, newS, v);
    }

    /// <summary>
    /// Adjusts the value of a color's HSV representation, and returns the adjusted color.
    /// If the adjustment would push the value past 0 or 1, it clamps to 0 or 1, respectively.
    /// </summary>
    /// <param name="color">The color to adjust.</param>
    /// <param name="amount">The amount to adjust by.</param>
    /// <param name="compensateSaturation">
    /// Whether or not to take adjustment overflow on value and use it to reduce saturation.
    /// Since HSV colors with full saturation cannot reach pure black or white, reducing saturation
    /// in some cases might be the only way to brighten/darken a color further.
    /// </param>
    /// <returns>
    /// The color with the same hue, an adjusted value, and possibly an adjusted saturation.
    /// </returns>
    public static Color AdjustValue(Color color, float amount, bool compensateSaturation = false)
    {
        color.ToHsv(out float h, out float s, out float v);

        float newV = v + amount;
        float newS = s;
        if (compensateSaturation)
        {
            if (newV > 1)
            {
                newS = Mathf.Clamp(s - (newV - 1), 0, 1);
            }
            else if (newV < 0)
            {
                newS = Mathf.Clamp(s - (0 - newV), 0, 1);
            }
        }

        newV = Mathf.Clamp(newV, 0, 1);

        return Color.FromHsv(h, newS, newV);
    }
}

public static class NumberOperations
{
    public static string ToOrdinal(int n)
    {
        int lastDigit = n % 10;
        if (lastDigit == 1)
        {
            int lastTwoDigits = n % 100;
            if (lastTwoDigits == 11 || lastTwoDigits == 12 || lastTwoDigits == 13)
            {
                return n + "th";
            }

            return n + "st";
        }
        else if (lastDigit == 2)
        {
            return n + "nd";
        }
        else if (lastDigit == 3)
        {
            return n + "rd";
        }
        else
        {
            return n + "th";
        }
    }
}