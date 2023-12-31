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
    [Export] private Sprite2D _flag;
    [Export] private Color _panelTint;
	[Export] private BlendModes _panelBlendMode;
    [Export] private Color _coverTint;
	[Export] private BlendModes _coverBlendMode;
    [Export] private Color _outlineTint;
	[Export] private BlendModes _outlineBlendMode;

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

    public void Init(MinesweeperGrid grid, int index, Color color)
    {
        _grid = grid;
        _index = index;

        _panel.Modulate = ColorOperations.Mix(_panelTint, color, _panelBlendMode);
        _cover.Modulate = ColorOperations.Mix(_coverTint, color, _coverBlendMode);
        _flag.Modulate = color;
        _hint.LabelSettings.Set("outline_color", ColorOperations.Mix(_outlineTint, color, _outlineBlendMode));
        _outline.Hide();
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
    }

    public void Deselect()
    {
        _outline.Hide();
    }
}
