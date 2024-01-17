using Godot;
using System;

public partial class AvatarAnimationData : Node
{
    [Export] private float _duration;
    [Export] private Curve _xDisplacement;
    [Export] private Curve _yDisplacement;
    [Export] private Curve _xScale;
    [Export] private Curve _yScale;
    [Export] private string _sprite;

    public float Duration => _duration;
    public Curve XDisplacement => _xDisplacement;
    public Curve YDisplacement => _yDisplacement;
    public Curve XScale => _xScale;
    public Curve YScale => _yScale;
    public string Sprite => _sprite;
}
