using Godot;

public partial class Flag : Node2D
{
    [Export] private Sprite2D _flagFill;

    private MinesweeperCell _cell;

    public void Init(MinesweeperCell cell)
    {
        _cell = cell;

        float h;
        float s;
        float v;
        cell.Window.PlayerColor.ToHsv(out h, out s, out v);

        // Get a complementary color with roughly the same saturation/brightness
        _flagFill.Modulate = Color.FromHsv((h + 0.5f) % 1, s, v);
    }
}
