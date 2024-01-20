using Godot;

public partial class Flag : Node2D
{
    [Export] private Sprite2D _flagFill;

    private MinesweeperCell _cell;

    public void Init(MinesweeperCell cell)
    {
        _cell = cell;

        _flagFill.Modulate = ColorOperations.AdjustHue(cell.Window.PlayerColor, 0.5f);
    }
}
