using Godot;
using System;

public partial class Countdown : Node
{
    [Signal]
    public delegate void CountdownFinishedEventHandler();

    [Export] private Timer _timer;
    [Export] private Label _label;
    
    private const int SECONDS = 3;
    private int _seconds;

    public override void _Ready()
    {
        base._Ready();

        _seconds = SECONDS;
        _label.Text = "" + SECONDS;
        _timer.Timeout += ProcessSecond; 
    }

    private void ProcessSecond()
    {
        _seconds -= 1;
        
        if (_seconds > 0)
        {
            _label.Text = "" + _seconds;
            _timer.Start();
        }
        else 
        {
            EmitSignal(SignalName.CountdownFinished);
            QueueFree();
        }
    }
}
