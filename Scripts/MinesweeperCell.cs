using Godot;
using System;

public enum MinesweeperCellState
{
    EMPTY,
    HINT,
    MINE,
    UNDECIDED
}

public partial class MinesweeperCell : ColorRect
{
    private MinesweeperCellState _state = MinesweeperCellState.UNDECIDED;
    private MinesweeperGrid _grid;
    private int _index;
    private bool _revealed;

    public bool IsMine => _state == MinesweeperCellState.MINE;
    public bool IsHint => _state == MinesweeperCellState.HINT;
    public bool IsEmpty => _state == MinesweeperCellState.EMPTY;

    public void Init(MinesweeperGrid grid, int index)
    {
        _grid = grid;
        _index = index;
    }

    public void AddMine()
    {
        if (_state != MinesweeperCellState.UNDECIDED) return;

        _state = MinesweeperCellState.MINE;

        // Fill some sort of image area with mine sprite
    }

    public void AddHint(int hint)
    {
        if (_state != MinesweeperCellState.UNDECIDED) return;

        if (hint == 0) _state = MinesweeperCellState.EMPTY;

        else
        {
            _state = MinesweeperCellState.HINT;
            // Fill some sort of text field with number
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

        else
        {
            // Reveal contents of cell

            return true;
        }
    }
}
