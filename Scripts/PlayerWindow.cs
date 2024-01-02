using Godot;
using System;

public partial class PlayerWindow : Node
{
    [Export] private PlayerInput _playerInput;
    [Export] private Control _gridCenter;
    [Export] private PackedScene _gridTemplate;

    public MinesweeperGrid Grid { get; private set; }
    public PlayerInput Input { get; private set; }
    public Color PlayerColor { get; private set; }
    public int PlayerID { get; private set; }

    public override void _Ready()
    {
        Init(1);
    }

    public void Init(int playerID)
    {
        Input = _playerInput;
        PlayerID = playerID;
        PlayerColor = Color.FromHsv(0, 1, 1);

        _playerInput.Init(this);

        SpawnBoard();
    }

    public void SpawnBoard()
    {
        try 
        {
            MinesweeperGrid grid = _gridTemplate.Instantiate() as MinesweeperGrid;

            grid.Init(this);
            _gridCenter.AddChild(grid);

            Grid = grid;
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
