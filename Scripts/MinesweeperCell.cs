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
    [Export] private Panel _panel;
    [Export] private Button _cover;
    [Export] private Label _hint;
    [Export] private Sprite2D _sprite;
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

    public bool IsMine => _state == MinesweeperCellState.MINE;
    public bool IsHint => _state == MinesweeperCellState.HINT;
    public bool IsEmpty => _state == MinesweeperCellState.EMPTY;

    public void Init(MinesweeperGrid grid, int index, Color color)
    {
        _grid = grid;
        _index = index;

        _panel.GetThemeStylebox("panel").Set("bg_color", ColorOperations.Mix(_panelTint, color, _panelBlendMode));
        _cover.GetThemeStylebox("normal").Set("bg_color", ColorOperations.Mix(_coverTint, color, _coverBlendMode));
        _hint.LabelSettings.Set("outline_color", ColorOperations.Mix(_outlineTint, color, _outlineBlendMode));
        _cover.ButtonDown += Click;
    }

    public void AddMine()
    {
        if (_state != MinesweeperCellState.UNDECIDED) return;

        _state = MinesweeperCellState.MINE;

        _hint.Text = "M";
    }

    public void AddHint(int hint)
    {
        if (_state != MinesweeperCellState.UNDECIDED) return;

        if (hint == 0) _state = MinesweeperCellState.EMPTY;

        else
        {
            _state = MinesweeperCellState.HINT;
            _hint.Text = "" + hint;
        }
    }

    public void Click()
    {
        if (_state == MinesweeperCellState.UNDECIDED)
        {
            _grid.Fill(_index);
        }

        _grid.Reveal(_index);
    }

    public bool Reveal()
    {
        if (_revealed) return false;

        _cover.Visible = false;
        _revealed = true;

        return true;
    }
}
