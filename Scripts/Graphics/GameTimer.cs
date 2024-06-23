using Godot;
using System;

public partial class GameTimer : Control
{
    [ExportGroup("References")]
    [Export] private Label _label;

    [ExportGroup("Parameters")]
    [Export] private Color _normalColor;
    [Export] private Color _hurryUpColor;
    [Export] private double _flashRate;
    [Export] private Curve _flashOpacity;

    public bool HurryUpMode 
    { 
        get { return _hurryUpMode; } 
        set {
            if (value) 
            {
                _label.LabelSettings.FontColor = _hurryUpColor;
                _hurryUpMode = true;
            }
            else 
            {
                _label.LabelSettings.FontColor = _normalColor;
                _hurryUpMode = false;
                _label.Modulate = new Color(_label.Modulate, 1);
            }
        } 
    }
    
    private bool _hurryUpMode;
    private double _flashTimer = 0;

    public void Display(double time) {
        _label.Text = "" + ((int) Math.Ceiling(time));
    }
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_hurryUpMode) 
        {
            _flashTimer += delta;

            double t = _flashTimer / _flashRate;
            
            if (t >= 1) 
            {
                _flashTimer = 0;
                t -= 1;
            }

            _label.Modulate = new Color(_label.Modulate, _flashOpacity.Sample((float) t));
        }
    }
}
