using Godot;
using System;

public partial class PlayerWindow : Control
{
    [ExportSubgroup("References")]
    [Export] private PlayerInput _playerInput;
    [Export] private PlayerAvatar _playerAvatar;
    [Export] private Control _squeezeBounds;
    [Export] private Label _scoreDisplay;
    [Export] private Label _mineDisplay;
    [Export] private Label _bonusDisplay;
    [Export] private PackedScene _gridTemplate;
    [Export] private GridLevelTrack _track;

    [ExportSubgroup("Parameters")]
    [Export] private int _squeezePaddingH;
    [Export] private int _squeezePaddingV;
    [Export] private float _zoomStartScale;
	[Export] private float _zoomIn;
    [Export] private float _boardSwitchDuration;
    [Export] private Curve _boardSwitchCurve;
    [Export] private float _fontValueAdjustment;

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
    public Color PlayerColor { get; private set; }
    public int PlayerID { get; private set; }
    public Vector2 SqueezeSize => new Vector2(_squeezeBounds.Size.X, _squeezeBounds.Size.Y);
    public float ZoomStartScale => _zoomStartScale;
    public float ZoomIn => _zoomIn;

    public override void _Ready()
    {
        Init(1);
    }

    public void Init(int playerID)
    {
        Input = _playerInput;
        PlayerID = playerID;
        PlayerColor = Color.FromHsv(0, 1, 1);

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

        ScoreToDisplay();

        _level = _track.StartLevel;
        if (_level < 0) throw new Exception("Level track does not have any levels!");

        _playerInput.Init(this);
        _playerAvatar.Init(this);

        SpawnNewGrid();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

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

            ActiveGrid.TryStart();
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

        if (win) 
        {
            _level = Mathf.Min(_level + 1, _track.Length - 1);
            _playerAvatar.PlayAnimation("HappyJump");
        }
        else
        {
            _level = Mathf.Max(_level - 1, 0);
            _playerAvatar.PlayAnimation("ScaredShake");
        } 
    }

    private void ScoreToDisplay()
    {
        _scoreDisplay.Text = _score + " pts";
    }
}

