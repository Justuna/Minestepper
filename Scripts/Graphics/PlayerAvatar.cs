using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerAvatar : Node2D
{
    [Export] private PackedScene _playerPrefab;
    [Export] private string _startingBaseSprite;

    private PlayerWindow _window;
    private PlayerMultiSprite _playerMultiSprite;
    private Dictionary<string, AvatarAnimationData> _animations = new Dictionary<string, AvatarAnimationData>();
    private AvatarAnimationData _currentAnimation = null;
    private double _animationProgress = 0;

    public string BaseSprite;

    public void Init(PlayerWindow window)
    {
        _window = window;
        BaseSprite = _startingBaseSprite;

        foreach (Node node in GetChildren())
        {
            try 
            {
                AvatarAnimationData animation = node as AvatarAnimationData;
                _animations.Add(animation.Name, animation);
            }
            catch (InvalidCastException) {}
        }

        try
        {
            _playerMultiSprite = _playerPrefab.Instantiate() as PlayerMultiSprite;

            _playerMultiSprite.Init(_window.PlayerColor);
            _playerMultiSprite.ShowSprite(BaseSprite);

            AddChild(_playerMultiSprite);
        }
        catch (InvalidCastException) 
        {
            GD.PrintErr("Player scene must have the PlayerMultiSprite script as its root type.");
        }
    }

    public override void _Process(double delta)
    {
        if (_currentAnimation != null)
        {
            _animationProgress += delta / _currentAnimation.Duration;

            if (_animationProgress >= 1)
            {
                _animationProgress = 0;
                _playerMultiSprite.Position = Vector2.Zero;
                _playerMultiSprite.Scale = Vector2.One;

                if (_playerMultiSprite.CurrentSprite != BaseSprite)
                {
                    _playerMultiSprite.ShowSprite(BaseSprite);
                }

                _currentAnimation = null;
            }
            else
            {
                float xPos = _currentAnimation.XDisplacement.Sample((float) _animationProgress);
                float yPos = _currentAnimation.YDisplacement.Sample((float) _animationProgress);
                float xScale = _currentAnimation.XScale.Sample((float) _animationProgress);
                float yScale = _currentAnimation.YScale.Sample((float) _animationProgress);

                _playerMultiSprite.Position = new Vector2(xPos, yPos);
                _playerMultiSprite.Scale = new Vector2(xScale, yScale);
            }
        }
    }

    public void PlayAnimation(string name)
    {
        bool found = _animations.TryGetValue(name, out AvatarAnimationData animation);
        if (!found) return;

        _animationProgress = 0;
        _currentAnimation = animation;
        _playerMultiSprite.ShowSprite(animation.Sprite);
    }
}
