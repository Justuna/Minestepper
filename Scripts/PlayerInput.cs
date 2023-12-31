using Godot;
using System;
using System.Linq;
using System.Net.Http.Headers;



public partial class PlayerInput : Node
{
    public const float NORMAL_SPEED = 1f;
    public const float FAST_SPEED = 2f;
    public const float DEADZONE = 0.2f;



    private int _playerID;
    private int _deviceID;

    private bool _fast = false;
    private bool _leftButton = false;
    private bool _rightButton = false;
    private bool _upButton = false;
    private bool _downButton = false;
    private bool _buttons => _leftButton || _rightButton || _upButton || _downButton;

    public void Init(int playerID, int deviceID)
    {
        _playerID = playerID;
        _deviceID = deviceID;
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

            if (joystick.Length() <= DEADZONE) velocity = Vector2.Zero;
            else
            {
                if (Math.Abs(joystick.X) > Math.Abs(joystick.Y)) velocity = new Vector2(Math.Abs(joystick.X) / joystick.X, 0);
                else velocity = new Vector2(0, Math.Abs(joystick.Y) / joystick.Y);
            }
        }

        velocity.X *= _fast ? FAST_SPEED : NORMAL_SPEED;
        velocity.Y *= _fast ? FAST_SPEED : NORMAL_SPEED;

        GD.Print(velocity);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.Device != _deviceID)
        {
            return;
        }

        if (@event.IsActionPressed("Fast"))
        {
            _fast = true;
        }
        else if (@event.IsActionReleased("Fast"))
        {
            _fast = false;
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

        if (@event.IsAction("Reveal"))
        {
            GD.Print("Reveal");
        }
    }
}