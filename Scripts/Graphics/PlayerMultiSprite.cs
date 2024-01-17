using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerMultiSprite : Node2D
{
    [Export] private string _startingSprite;

    private Dictionary<string, PlayerSprite> _sprites = new Dictionary<string, PlayerSprite>();
    private PlayerSprite _currentSprite;

    public string CurrentSprite => (_currentSprite != null) ? _currentSprite.Name : "";

    public void Init(Color color)
    {
        foreach (Node node in GetChildren())
        {
            try 
            {
                PlayerSprite sprite = node as PlayerSprite;
                _sprites.Add(sprite.Name, sprite);

                if (sprite.Name != _startingSprite) sprite.Visible = false;
                else _currentSprite = sprite;

                sprite.ColorFill(color);
            }
            catch (InvalidCastException) {}
        }
    }

    public void Recolor(Color color)
    {
        foreach (Node node in GetChildren())
        {
            try 
            {
                PlayerSprite sprite = node as PlayerSprite;
                sprite.ColorFill(color);
            }
            catch (InvalidCastException) {}
        }
    }

    public void ShowSprite(string name)
    {
        if (_currentSprite == null || _currentSprite.Name != name) 
        {
            bool found = _sprites.TryGetValue(name, out PlayerSprite newSprite);
            if (!found) return;

            _currentSprite.Visible = false;
            newSprite.Visible = true;

            _currentSprite = newSprite;
        }
    }
}
