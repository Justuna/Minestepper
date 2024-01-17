using Godot;
using System;
using System.Linq;
using System.Net.Http.Headers;



public partial class PlayerInput : Node
{
    [Export] private float _deadZone = 0.2f;

    public Vector2 Direction = Vector2.Zero;
    public bool Fast { get; private set; } = false;

    private PlayerWindow _window;
    private int _deviceID;
    private bool _leftButton = false;
    private bool _rightButton = false;
    private bool _upButton = false;
    private bool _downButton = false;

    public void Init(PlayerWindow window)
    {
        _window = window;

        try 
        {
            _deviceID = Input.GetConnectedJoypads()[_window.PlayerID - 1];
        }
        catch (Exception)
        {
            GD.Print("Oops");

            _deviceID = -1;

            if (_window.PlayerID <= 0)
            {
                // Negative player means this player viewport is not being used
                // Zero player is unused
            }
            else
            {
                // Something went wrong with the controller
            }
        }
    }

    public override void _Process(double delta)
    {
        Vector2 velocity;

        if (_leftButton)
        {
            velocity = Vector2.Left;
        }
        else if (_rightButton)
        {
            velocity = Vector2.Right;
        }
        else if (_upButton)
        {
            velocity = Vector2.Up;
        }
        else if (_downButton)
        {
            velocity = Vector2.Down;
        }
        else
        {
            Vector2 joystick = new Vector2(Input.GetJoyAxis(_deviceID, JoyAxis.LeftX), Input.GetJoyAxis(_deviceID, JoyAxis.LeftY));

            if (joystick.Length() <= _deadZone) velocity = Vector2.Zero;
            else
            {
                if (Math.Abs(joystick.X) > Math.Abs(joystick.Y)) velocity = new Vector2(Math.Abs(joystick.X) / joystick.X, 0);
                else velocity = new Vector2(0, Math.Abs(joystick.Y) / joystick.Y);
            }
        }

        Direction = velocity;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.Device != _deviceID)
        {
            return;
        }

        if (@event.IsActionPressed("Fast"))
        {
            Fast = true;
        }
        else if (@event.IsActionReleased("Fast"))
        {
            Fast = false;
        }

        if (@event.IsActionPressed("Left"))
        {
            _leftButton = true;
        }
        else if (@event.IsActionReleased("Left"))
        {
            _leftButton = false;
        }

        if (@event.IsActionPressed("Right"))
        {
            _rightButton = true;
        }
        else if (@event.IsActionReleased("Right"))
        {
            _rightButton = false;
        }

        if (@event.IsActionPressed("Up"))
        {
            _upButton = true;
        }
        else if (@event.IsActionReleased("Up"))
        {
            _upButton = false;
        }

        if (@event.IsActionPressed("Down"))
        {
            _downButton = true;
        }
        else if (@event.IsActionReleased("Down"))
        {
            _downButton = false;
        }

        if (@event.IsActionPressed("Reveal"))
        {
            if (_window.ActiveGrid != null) _window.ActiveGrid.Reveal();
        }

        if (@event.IsActionPressed("Flag"))
        {
            if (_window.ActiveGrid != null) _window.ActiveGrid.Flag();
        }
    }
}