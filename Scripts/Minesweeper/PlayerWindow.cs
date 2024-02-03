using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class PlayerWindow : Control
{
    [ExportGroup("References")]
    [Export] private PlayerInput _playerInput;
    [Export] private PlayerAvatar _playerAvatar;
    [Export] private Control _squeezeBounds;
    [Export] private Control _pointChangeTickSpawn;
    [Export] private PackedScene _pointChangeTick;
    [Export] private Label _scoreDisplay;
    [Export] private Label _mineDisplay;
    [Export] private Label _bonusDisplay;
    [Export] private Label _flagDisplay;
    [Export] private Flag _flagIcon;
    [Export] private PackedScene _gridTemplate;
    [Export] private GridLevelTrack _track;
    [Export] private AnimatedSprite2D[] _backgrounds;

    [ExportGroup("Parameters")]
    [Export] private float _boardSwitchDuration;
    [Export] private Curve _boardSwitchCurve;
    [Export] private float _fontValueAdjustment;
    [Export] private PackedScene _playerSpritePrefab;
    [Export] private Vector2 _smallThreshold;
    [Export] private Color _playerColor;
    
    [ExportSubgroup("Large UI")]
    [Export] private float _playerSpriteScaleLarge;
    [Export] private int _fontLarge;
    [Export] private int _fontOutlineLarge;
    
    [ExportSubgroup("Small UI")]
    [Export] private float _playerSpriteScaleSmall;
    [Export] private int _fontSmall;
    [Export] private int _fontOutlineSmall;

    private Vector2 _screenCenter => new Vector2(Size.X / 2, Size.Y / 2);
    private Vector2 _offscreenTop => _screenCenter + new Vector2(0, -2 * Size.Y);
    private Vector2 _offscreenBottom => _screenCenter + new Vector2(0, 2 * Size.Y);
    private MinesweeperGrid _oldGrid;
    private MinesweeperGrid _newGrid;
    private double _gridSwitchProgress = 1;
    private int _score = 0;
    private int _level;

    public MinesweeperGrid ActiveGrid { get; private set; }
    public PlayerInput Input { get; private set; }
    public PackedScene PlayerSpritePrefab => _playerSpritePrefab;
    public Color PlayerColor => _playerColor;
    public int PlayerID { get; private set; }
    public Vector2 SqueezeSize => new Vector2(_squeezeBounds.Size.X, _squeezeBounds.Size.Y);

    public override void _Ready()
    {
        Init(1);
    }

    public void Init(int playerID)
    {
        Input = _playerInput;
        PlayerID = playerID;

        Color fontOutline = ColorOperations.AdjustValue(PlayerColor, _fontValueAdjustment, true);

        _scoreDisplay.LabelSettings = _scoreDisplay.LabelSettings.Duplicate() as LabelSettings;
        _bonusDisplay.LabelSettings = _scoreDisplay.LabelSettings.Duplicate() as LabelSettings;
        _mineDisplay.LabelSettings = _scoreDisplay.LabelSettings.Duplicate() as LabelSettings;
        _flagDisplay.LabelSettings = _scoreDisplay.LabelSettings.Duplicate() as LabelSettings;

        _scoreDisplay.LabelSettings.OutlineColor = fontOutline;
        _bonusDisplay.LabelSettings.OutlineColor = fontOutline;
        _mineDisplay.LabelSettings.OutlineColor = fontOutline;
        _flagDisplay.LabelSettings.OutlineColor = fontOutline;

        foreach (AnimatedSprite2D background in _backgrounds)
        {
            background.Play("default");
            background.Modulate = new Color(PlayerColor, background.Modulate.A);
        }   

        GetTree().Root.SizeChanged += Resize;
        Resize();

        _flagIcon.Init(this);

        ScoreToDisplay();

        _level = _track.StartLevel;
        if (_level < 0) throw new Exception("Level track does not have any levels!");

        _playerInput.Init(this);
        _playerAvatar.Init(this);

        // Need to wait for the controls to actually establish their sizes
        NextTick(SpawnNewGrid);
    }

    private Queue<Action> _nextTickActions = new();
    private void NextTick(Action action)
    {
        _nextTickActions.Enqueue(action);
    }

    public void Resize()
    {
        NextTick(() => 
        {
            foreach (AnimatedSprite2D background in _backgrounds)
            {
                background.Scale = Size / background.SpriteFrames.GetFrameTexture("default", 0).GetSize();
            }   

            if (GetTree().Root.Size.X >= _smallThreshold.X && GetTree().Root.Size.Y >= _smallThreshold.Y)
            {
                _playerAvatar.Scale = new Vector2(_playerSpriteScaleLarge, _playerSpriteScaleLarge);

                _scoreDisplay.LabelSettings.FontSize = _fontLarge;
                _bonusDisplay.LabelSettings.FontSize = _fontLarge;
                _mineDisplay.LabelSettings.FontSize = _fontLarge;
                _flagDisplay.LabelSettings.FontSize = _fontLarge;

                _scoreDisplay.LabelSettings.OutlineSize = _fontOutlineLarge;
                _bonusDisplay.LabelSettings.OutlineSize = _fontOutlineLarge;
                _mineDisplay.LabelSettings.OutlineSize = _fontOutlineLarge;
                _flagDisplay.LabelSettings.OutlineSize = _fontOutlineLarge;
            }
            else 
            {
                _playerAvatar.Scale = new Vector2(_playerSpriteScaleSmall, _playerSpriteScaleSmall);

                _scoreDisplay.LabelSettings.FontSize = _fontSmall;
                _bonusDisplay.LabelSettings.FontSize = _fontSmall;
                _mineDisplay.LabelSettings.FontSize = _fontSmall;
                _flagDisplay.LabelSettings.FontSize = _fontSmall;

                _scoreDisplay.LabelSettings.OutlineSize = _fontOutlineSmall;
                _bonusDisplay.LabelSettings.OutlineSize = _fontOutlineSmall;
                _mineDisplay.LabelSettings.OutlineSize = _fontOutlineSmall;
                _flagDisplay.LabelSettings.OutlineSize = _fontOutlineSmall;
            }

            
        });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        while (_nextTickActions.Count > 0) _nextTickActions.Dequeue().Invoke();

        if (_gridSwitchProgress < 1) SwitchGridAnimation(delta);
    }

    private void SwitchGridAnimation(double delta)
    {
        _gridSwitchProgress += delta / _boardSwitchDuration;
        float t = _boardSwitchCurve.Sample((float) _gridSwitchProgress);

        if (_oldGrid != null)
        {
            // Move old grid from center position upward so that it goes offscreen
            _oldGrid.Position = (1 - t) * _screenCenter + t * _offscreenTop;
        }
        if (_newGrid != null)
        {
            // Move new grid from position offscreen into center
            _newGrid.Position = (1 - t) * _offscreenBottom + t * _screenCenter;
        }

        if (_gridSwitchProgress >= 1)
        {
            _gridSwitchProgress = 1;

            // Animation finished, delete old grid now that it is offscreen
            if (_oldGrid != null) _oldGrid.QueueFree();

            ActiveGrid = _newGrid;
            _newGrid = null;
            _oldGrid = null;

            GridLevel level = _track.GetLevel(_level);

            _mineDisplay.Text = $"{level.GridMineCount} mines, +{level.CorrectWorth}/-{level.MinePenalty} per";
            _bonusDisplay.Text = $"Clear Bonus: +{level.ClearBonus}";
            _flagDisplay.Text = $"0/{level.GridMineCount}";
            _flagDisplay.Visible = true;

            ActiveGrid.Start();
        }
    }

    private void SpawnNewGrid()
    {
        try 
        {
            _newGrid = _gridTemplate.Instantiate() as MinesweeperGrid;

            GridLevel level = _track.GetLevel(_level);
            _newGrid.Init(this, level.GridWidth, level.GridHeight, level.GridMineCount);
            AddChild(_newGrid);

            _newGrid.Position = _offscreenBottom;
            
            _newGrid.GridGameOver += ResolveGrid;
            _newGrid.GridAnimationComplete += SpawnNewGrid;
            _newGrid.GridFlagNumberChanged += FlagDisplayChange;
            
            // Prepare for animation
            _oldGrid = ActiveGrid;
            ActiveGrid = null;

            // Trigger animation start
            _gridSwitchProgress = 0;
        }
        catch (InvalidCastException)
        {
            GD.PrintErr("Board scene must have MinesweeperGrid script as its root type.");
        }
    }

    private void ResolveGrid(bool win)
    {
        GridLevel level = _track.GetLevel(_level);

        int scoreChange = ((ActiveGrid.TotalMines - ActiveGrid.UnflaggedMines) * level.CorrectWorth) - (ActiveGrid.UnflaggedMines * level.MinePenalty);
        if (ActiveGrid.UnflaggedMines == 0) scoreChange += level.ClearBonus;

        _score += scoreChange;
        ScoreToDisplay();

        Node node = _pointChangeTick.Instantiate();
        if (node is PointChangeTick)
        {
            PointChangeTick tick = node as PointChangeTick;
            tick.Init(scoreChange);

            _pointChangeTickSpawn.AddChild(tick);
        }
        else
        {
            GD.PrintErr("Point change tick prefab must have the PointChangeTick script as its root node.");
        }

        if (win) 
        {
            _level = Mathf.Min(_level + 1, _track.Length - 1);
            _playerAvatar.PlayAnimation("HappyJump");
            _flagDisplay.Text = $"{ActiveGrid.TotalMines}/{ActiveGrid.TotalMines}!!";
        }
        else
        {
            _level = Mathf.Max(_level - 1, 0);
            _playerAvatar.PlayAnimation("ScaredShake");
            _flagDisplay.Text = $"{ActiveGrid.TotalMines - ActiveGrid.UnflaggedMines}/{ActiveGrid.TotalMines}...";
        } 
    }

    private void ScoreToDisplay()
    {
        _scoreDisplay.Text = _score + " pts";
    }

    private void FlagDisplayChange(int flags)
    {
        GridLevel level = _track.GetLevel(_level);

        _flagDisplay.Text = $"{flags}/{level.GridMineCount}";
        if (flags == level.GridMineCount) _flagDisplay.Text += "?";
        else if (flags > level.GridMineCount) _flagDisplay.Text += "??";
    }
}

