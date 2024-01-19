using Godot;

[GlobalClass]
public partial class GridLevel : Resource
{
    [Export] private int _gridWidth;
    [Export] private int _gridHeight;
    [Export] private int _gridMineCount;
    [Export] private int _correctWorth;
    [Export] private int _minePenalty;
    [Export] private int _clearBonus;

    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public int GridMineCount => _gridMineCount;
    public int CorrectWorth => _correctWorth;
    public int MinePenalty => _minePenalty;
    public int ClearBonus => _clearBonus;

    public GridLevel()
    {
        _gridWidth = 0;
        _gridHeight = 0;
        _gridMineCount = 0;
        _correctWorth = 0;
        _minePenalty = 0;
        _clearBonus = 0;
    }

    public GridLevel(int width, int height, int mineCount, int correctWorth, int minePenalty, int clearBonus)
    {
        _gridWidth = width;
        _gridHeight = height;
        _gridMineCount = mineCount;
        _correctWorth = correctWorth;
        _minePenalty = minePenalty;
        _clearBonus = clearBonus;
    }
}