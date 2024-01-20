using Godot;
using System;

public enum MinesweeperCellState
{
    EMPTY,
    HINT,
    MINE,
    UNDECIDED
}

public partial class MinesweeperCell : Control
{
    [Export] private Panel _outline;
    [Export] private Panel _panel;
    [Export] private Panel _cover;
    [Export] private Label _hint;
    [Export] private Sprite2D _bomb;
    [Export] private Color _defuseTint;
    [Export] private Flag _flag;
    [Export] private float _panelValueAdjustment;
    [Export] private float _coverValueAdjustment;
    [Export] private float _outlineValueAdjustment;

    private MinesweeperCellState _state = MinesweeperCellState.UNDECIDED;
    private MinesweeperGrid _grid;
    private int _index;
    private bool _revealed;
    private bool _flagged;

    public bool IsMine => _state == MinesweeperCellState.MINE;
    public bool IsHint => _state == MinesweeperCellState.HINT;
    public bool IsEmpty => _state == MinesweeperCellState.EMPTY;
    public bool IsDecided => _state != MinesweeperCellState.UNDECIDED;
    public bool IsFlagged => _flagged;
    public bool IsRevealed => _revealed;
    public int HintValue { get; private set; }

    public PlayerWindow Window => _grid.Window;

    public void Init(MinesweeperGrid grid, int index)
    {
        _grid = grid;
        _index = index;

        _panel.Modulate = ColorOperations.AdjustValue(Window.PlayerColor, _panelValueAdjustment, true);
        _cover.Modulate = ColorOperations.AdjustValue(Window.PlayerColor, _coverValueAdjustment, true);
        _hint.LabelSettings.Set("outline_color", ColorOperations.AdjustValue(Window.PlayerColor, _outlineValueAdjustment, true));
        _outline.Hide();

        _flag.Init(this);
    }

    public void AddMine()
    {
        if (_state != MinesweeperCellState.UNDECIDED) return;

        _state = MinesweeperCellState.MINE;
        _bomb.Visible = true;
    }

    public void AddHint(int hint)
    {
        if (_state != MinesweeperCellState.UNDECIDED) return;

        if (hint == 0) _state = MinesweeperCellState.EMPTY;

        else
        {
            _state = MinesweeperCellState.HINT;
            HintValue = hint;
            _hint.Text = "" + hint;
            _hint.Visible = true;
        }
    }

    public bool Reveal()
    {
        if (_revealed) return false;

        _cover.Visible = false;
        _revealed = true;

        return true;
    }

    public void Select()
    {
        _outline.Show();
    }

    public void Flag()
    {
        if (_revealed) return;

        _flagged = !_flagged;
        _flag.Visible = _flagged;

        if (_flagged)
        {
            _bomb.Modulate = _defuseTint;
        }
        else
        {
            _bomb.Modulate = Colors.White;
        }
    }

    public void Deselect()
    {
        _outline.Hide();
    }
}
