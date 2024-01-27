using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

public partial class PlayerWindow : Control
{
    [ExportSubgroup("References")]
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

    [ExportSubgroup("Parameters")]
    [Export] private int _squeezePaddingH;
    [Export] private int _squeezePaddingV;
    [Export] private float _boardSwitchDuration;
    [Export] private Curve _boardSwitchCurve;
    [Export] private float _fontValueAdjustment;
    [Export] private PackedScene _playerAvatarPrefab;
    [Export] private Color _playerColor;

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
    public PackedScene PlayerAvatarPrefab => _playerAvatarPrefab;
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

        float maxWidth = Size.X - (_squeezePaddingH * 2);
        float maxHeight = Size.Y - (_squeezePaddingV * 2);
        if (maxWidth < maxHeight)
        {
            _squeezeBounds.Size = new Vector2(maxWidth, maxWidth);
            _squeezeBounds.Position = new Vector2((Size.X - maxWidth) / 2, (Size.Y - maxWidth) / 2);
            _squeezeBounds.PivotOffset = new Vector2(maxWidth / 2, maxWidth / 2);
        }
        else
        {
            _squeezeBounds.Size = new Vector2(maxHeight, maxHeight);
            _squeezeBounds.Position = new Vector2((Size.X - maxHeight) / 2, (Size.Y - maxHeight) / 2);
            _squeezeBounds.PivotOffset = new Vector2(maxHeight / 2, maxHeight / 2);
        }

        Color fontOutline = ColorOperations.AdjustValue(PlayerColor, _fontValueAdjustment, true);
        _scoreDisplay.LabelSettings.OutlineColor = fontOutline;
        _bonusDisplay.LabelSettings.OutlineColor = fontOutline;
        _mineDisplay.LabelSettings.OutlineColor = fontOutline;
        _flagDisplay.LabelSettings.OutlineColor = fontOutline;

        foreach (AnimatedSprite2D background in _backgrounds)
        {
            background.Play("default");
            background.Modulate = new Color(PlayerColor, background.Modulate.A);
        }   

        GetTree().Root.SizeChanged += SetUpBackgrounds;
        SetUpBackgrounds();

        _flagIcon.Init(this);

        ScoreToDisplay();

        _level = _track.StartLevel;
        if (_level < 0) throw new Exception("Level track does not have any levels!");

        _playerInput.Init(this);
        _playerAvatar.Init(this);

        SpawnNewGrid();
    }

    private Queue<Action> _nextTickActions = new();
    private void NextTick(Action action)
    {
        _nextTickActions.Enqueue(action);
    }

    public void SetUpBackgrounds()
    {
        NextTick(() => 
        {
            foreach (AnimatedSprite2D background in _backgrounds)
            {
                GD.Print(Size);
                GD.Print(background.SpriteFrames.GetFrameTexture("default", 0).GetSize());
                background.Scale = Size / background.SpriteFrames.GetFrameTexture("default", 0).GetSize();
            }   
        });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        foreach (Action action in _nextTickActions)
        {
            action.Invoke();
        }

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

