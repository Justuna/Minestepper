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
    [ExportGroup("References")]
    [Export] private PlayerWindow[] _playerWindows;
    [Export] private GameTimer _timer;

    [ExportGroup("Parameters")]
    [Export] private double _hurryUpTime = 10;
    
    private EntryPoint _entryPoint;
    private double _timeLeft;
    private bool _gameOver = false;

    public void Init(EntryPoint entryPoint, List<PlayerData> data, double time)
    {
        _entryPoint = entryPoint;
        _timeLeft = time;
        _timer.Display(_timeLeft);

        for (int i = 0; i < _playerWindows.Length; i++)
        {
            _playerWindows[i].Init(data[i]);
        }
    }

    public override void _Process(double delta) 
    {
        if (!_gameOver) 
        {
            _timeLeft -= delta;
            _timer.Display(_timeLeft);

            if (_timeLeft <= _hurryUpTime && !_timer.HurryUpMode)
            {
                _timer.HurryUpMode = true;
            }

            if (_timeLeft <= 0) 
            {
                GameEnd();
            }
        }
    }

    void GameEnd() 
    {
        _gameOver = true;
        _timer.HurryUpMode = false;
        _timer.Display(0);

        for (int i = 0; i < _playerWindows.Length; i++)
        {
            // _playerWindows[i].
        }
    }
}
