using Godot;

public partial class Flag : Control
{
    [Export] private TextureRect _flagFill;

    private MinesweeperCell _cell;
    private PlayerWindow _window;

    public void Init(MinesweeperCell cell)
    {
        _cell = cell;
        _window = cell.Window;

        SetColor();
    }

    public void Init(PlayerWindow window)
    {
        _window = window;

        SetColor();
    }

    public void SetColor()
    {
        _flagFill.Modulate = ColorOperations.AdjustHue(_window.PlayerColor, 0.5f);
    }
}
