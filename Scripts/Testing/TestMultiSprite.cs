using Godot;
using System;

public partial class TestMultiSprite : Node2D
{
    [Export] private ColorPickerButton _colorPicker;
    [Export] private Button _idle;
    [Export] private Button _excited;
    [Export] private Button _freakout;
    [Export] private Button _disappointed;
    [Export] private PlayerMultiSprite _player;

    public override void _Ready()
    {
        base._Ready();
        
        _player.Init(_colorPicker.Color);
        _colorPicker.ColorChanged += color => _player.Recolor(color);
        
        _idle.Pressed += () => _player.ShowSprite("Idle");
        _excited.Pressed += () => _player.ShowSprite("Excited");
        _freakout.Pressed += () => _player.ShowSprite("Freakout");
        _disappointed.Pressed += () => _player.ShowSprite("Disappointed");
    }

}
