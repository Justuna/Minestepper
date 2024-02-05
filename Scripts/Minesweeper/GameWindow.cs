using Godot;
using System;
using System.Collections.Generic;

public struct PlayerData
{
    public int index;
    public int id => index + 1;
    public int score;
    public Color color;
}

public partial class GameWindow : Control
{
    [Export] private PlayerWindow[] _playerWindows;
    private EntryPoint _entryPoint;

    public void Init(EntryPoint entryPoint, List<PlayerData> data)
    {
        _entryPoint = entryPoint;

        for (int i = 0; i < _playerWindows.Length; i++)
        {
            _playerWindows[i].Init(data[i]);
        }
    }
}
