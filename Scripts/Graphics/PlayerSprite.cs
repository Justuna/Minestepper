using Godot;
using System;

public partial class PlayerSprite : Node2D
{
    [Export] private Sprite2D[] _fills;

    public void ColorFill(Color color)
    {
        foreach(Sprite2D fill in _fills)
        {
            fill.Modulate = color;
        }
    }
}
