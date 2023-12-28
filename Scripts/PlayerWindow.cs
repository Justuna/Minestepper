using Godot;
using System;

public partial class PlayerWindow : Node
{
    [Export] private Control _gridCenter;
    [Export] private PackedScene _gridTemplate;

    public override void _Ready()
    {
        Init();

        base._Ready();
    }

    public void Init()
    {
        SpawnBoard();
    }

    public void SpawnBoard()
    {
        try 
        {
            MinesweeperGrid grid = _gridTemplate.Instantiate() as MinesweeperGrid;

            grid.Init(Color.FromHsv(0, 1, 1), 5, 5, 4);
            _gridCenter.AddChild(grid);
        }
        catch (InvalidCastException)
        {
            GD.PrintErr("Board scene must have MinesweeperGrid script as its root type.");
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }
}
