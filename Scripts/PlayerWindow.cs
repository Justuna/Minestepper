using Godot;
using System;

public partial class PlayerWindow : Control
{
    [ExportSubgroup("References")]
    [Export] private PlayerInput _playerInput;
    [Export] private Control _squeezeBounds;
    [Export] private PackedScene _gridTemplate;

    [ExportSubgroup("Parameters")]
    [Export] private int _squeezePaddingH;
    [Export] private int _squeezePaddingV;
    [Export] private float _zoomStartScale;
	[Export] private float _zoomIn;

    public MinesweeperGrid Grid { get; private set; }
    public PlayerInput Input { get; private set; }
    public Color PlayerColor { get; private set; }
    public int PlayerID { get; private set; }
    public Vector2 SqueezeSize => new Vector2(_squeezeBounds.Size.X, _squeezeBounds.Size.Y);
    public float ZoomStartScale => _zoomStartScale;
    public float ZoomIn => _zoomIn;

    public override void _Ready()
    {
        Init(1);
    }

    public void Init(int playerID)
    {
        Input = _playerInput;
        PlayerID = playerID;
        PlayerColor = Color.FromHsv(0, 1, 1);

        float maxWidth = Size.X - (_squeezePaddingH * 2);
        float maxHeight = Size.Y - (_squeezePaddingV * 2);
        if (maxWidth < maxHeight)
        {
            _squeezeBounds.Size = new Vector2(maxWidth, maxWidth);
            _squeezeBounds.Position = new Vector2((Size.X - maxWidth) / 2, (Size.Y - maxWidth) / 2);
            _squeezeBounds.PivotOffset = new Vector2(maxWidth / 2, maxWidth / 2);
        }
        else
        {
            _squeezeBounds.Size = new Vector2(maxHeight, maxHeight);
            _squeezeBounds.Position = new Vector2((Size.X - maxHeight) / 2, (Size.Y - maxHeight) / 2);
            _squeezeBounds.PivotOffset = new Vector2(maxHeight / 2, maxHeight / 2);
        }

        _playerInput.Init(this);

        SpawnBoard();
    }

    public void SpawnBoard()
    {
        if (Grid != null)
        {
            MinesweeperGrid old = Grid;
            Grid = null;

            old.QueueFree();
        }
        try 
        {
            MinesweeperGrid grid = _gridTemplate.Instantiate() as MinesweeperGrid;

            grid.Init(this, 10, 10, 10);
            _squeezeBounds.AddChild(grid);

            Grid = grid;
            grid.GridFinished += (won) => 
            {
                SpawnBoard();
            };
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
