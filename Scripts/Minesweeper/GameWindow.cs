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
    [Export] private GameTimer _gameTimer;
    [Export] private Countdown _countdown;
    [Export] private Timer _resultTimer;
    [Export] private Timer _closeGameTimer;

    [ExportGroup("Parameters")]
    [Export] private double _hurryUpTime = 10;
    
    private EntryPoint _entryPoint;
    private double _timeLeft;
    private bool _gameRunning = false;

    public void Init(EntryPoint entryPoint, List<PlayerData> data, double time)
    {
        _entryPoint = entryPoint;
        _timeLeft = time;
        _gameTimer.Display(_timeLeft);

        for (int i = 0; i < _playerWindows.Length; i++)
        {
            _playerWindows[i].Init(data[i]);
        }

        _countdown.CountdownFinished += Start;
        _resultTimer.Timeout += Results;
    }

    private void Start()
    {
        _gameRunning = true;

        for (int i = 0; i < _playerWindows.Length; i++)
        {
            _playerWindows[i].Start();
        }
    }

    public override void _Process(double delta) 
    {
        if (_gameRunning) 
        {
            _timeLeft -= delta;
            _gameTimer.Display(_timeLeft);

            if (_timeLeft <= _hurryUpTime && !_gameTimer.HurryUpMode)
            {
                _gameTimer.HurryUpMode = true;
            }

            if (_timeLeft <= 0) 
            {
                GameEnd();
            }
        }
    }

    void GameEnd() 
    {
        _gameRunning = false;
        _gameTimer.HurryUpMode = false;
        _gameTimer.Display(0);

        for (int i = 0; i < _playerWindows.Length; i++)
        {
            _playerWindows[i].End();
        }

        _resultTimer.Start();
    }

    void Results()
    {
        PlayerWindow[] sorted = (PlayerWindow[]) _playerWindows.Clone();
        Array.Sort(sorted);

        var points = new Godot.Collections.Array<Godot.Collections.Dictionary>();

        int place = 0;
        int prevScore = 0;
        foreach (PlayerWindow window in sorted)
        {
            if (place == 0 || prevScore != window.Score) 
            {
                place += 1;
            }

            if (place == 1) 
            {
                window.Results(true, "Winner!");
            }
            else 
            {
                window.Results(false, NumberOperations.ToOrdinal(place) + " Place...");
            }

            prevScore = window.Score;

            var playerData = new Godot.Collections.Dictionary
            {
                { "player", window.PlayerIndex },
                { "points", PointsFromPlacement(place) }
            };
            points.Add(playerData);
        }

        _closeGameTimer.Timeout += () => 
        {
            _entryPoint.MinigameManager.Call("end_game", points);
        };
        _closeGameTimer.Start();
    }

    int PointsFromPlacement(int place)
    {
        if (place == 1) return 5;
        else if (place == 2) return 3;
        else if (place == 3) return 1;
        else return 0;
    }
}
