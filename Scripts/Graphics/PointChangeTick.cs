using Godot;
using System;

public partial class PointChangeTick : Label
{
    [Export] private float _lifetime;
    [Export] private Vector2 _velocity = Vector2.Up;
    [Export] private Curve _velocityScaleCurve;
    [Export] private Curve _opacityCurve;

    private float t = 0;

    public void Init(int amount)
    {
        if (amount < 0)
        {
            Text = "" + amount;
        }
        else
        {
            Text = "+" + amount;
        } 
    }

    public override void _Process(double delta)
    {
        t += (float) delta / _lifetime;

        Position += (float) (_velocityScaleCurve.Sample(t) * delta) * _velocity;
        Modulate = new Color(1, 1, 1, Mathf.Clamp(_opacityCurve.Sample(t), 0, 1));

        if (t >= 1) QueueFree();
    }
}
