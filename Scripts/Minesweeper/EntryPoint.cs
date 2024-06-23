using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

public partial class EntryPoint : Node
{
    [ExportGroup("References")]
    [Export] private PackedScene _2PlayerScene;
    [Export] private PackedScene _3PlayerScene;
    [Export] private PackedScene _4PlayerScene;
    [Export] private Node _minigameManager;

    [ExportGroup("Parameters")]
    [Export] private double _gameLength;

    public GodotObject MinigameManager => _minigameManager;

    public void Init(Variant playersVariant)
    {
        GD.Print("Initializing players...");

        Array players = playersVariant.As<Array>();

        GD.Print($"Found {players.Count} players!");

        List<PlayerData> data = new List<PlayerData>();

        for (int i = 0; i < players.Count; i++)
        {
            try
            {
                GD.Print($"Reading player {i + 1}...");

                Variant playerVariant = players[i];
                GodotObject player = playerVariant.As<GodotObject>();

                Color color = player.Get("color").As<Color>();
                int score = (int) player.Get("score").As<long>();

                GD.Print($"Player is color {color}!");

                data.Add(new PlayerData
                {
                    index = i,
                    score = score,
                    color = color
                });
            }
            catch (Exception) 
            {
                GD.PrintErr("Received variant data in unexpected format...");
            }
        }

        GameWindow window;

        if (players.Count == 2)
        {
            GD.Print("Setting up for 2...");
            window = _2PlayerScene.Instantiate<GameWindow>();
        }
        else if (players.Count == 3)
        {
            GD.Print("Setting up for 3...");
            window = _3PlayerScene.Instantiate<GameWindow>();
        }
        else if (players.Count == 4)
        {
            GD.Print("Setting up for 4...");
            window = _4PlayerScene.Instantiate<GameWindow>();
        }
        else
        {
            throw new Exception("This game only supports 2-4 players!");
        }
         
        AddChild(window);
        window.Init(this, data, _gameLength);
    }
}
