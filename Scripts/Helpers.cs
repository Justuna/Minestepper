using Godot;

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
}
