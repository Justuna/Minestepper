using Godot;
using System;
using System.Collections.Generic;

public partial class MinesweeperGrid : Control
{
	[ExportSubgroup("References")]
	[Export] private Control _gridAnchor;
	[Export] private Panel _background;
	[Export] private GridContainer _gridLayout;
	[Export] private PackedScene _gridCellTemplate;

	[ExportSubgroup("Visual Parameters")]
	[Export] private int _spacing;
	[Export] private int _padding;
	[Export] private Color _bgTint;
	[Export] private BlendModes _bgBlendMode;

	[ExportSubgroup("Animation Parameters")]
	[Export] private float _zoomDuration;
	[Export] private Curve _zoomCurve;
	[Export] private float _zoomPanMinSpeed;
	[Export] private float _zoomPanMaxSpeed;

	[ExportSubgroup("Gameplay Parameters")]
	[Export] private float _speed;
	[Export] private float _fastFactor;

	private const int DEFAULT_WIDTH = 6;
	private const int DEFAULT_HEIGHT = 6;
	private const int DEFAULT_MINE_COUNT = 4;

	private PlayerWindow _window;
	private MinesweeperCell[] _gridCells;
	private int _width;
	private int _height;
	private int _mineCount;
	private int _revealed = 0;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private int _area => _width * _height;
	private bool _active = false;

	private Vector2 _direction;
	private int _selected;
	private double _moveProgress = 0;

	private bool _canZoom = false;
	private bool _shouldZoom = false;
	private bool _isZooming = false;
	private double _zoomProgress = 0;
	private Vector2 _cellOffset;
	private Vector2 _zoomPos = Vector2.Zero;
	

	[Signal]
	public delegate void GridGameOverEventHandler(bool win);
	[Signal]
	public delegate void GridAnimationCompleteEventHandler();

	public PlayerWindow Window => _window;
	public int TotalMines => _mineCount;
	public int UnflaggedMines { get; private set; }

    public void Init(PlayerWindow window, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT, int mineCount = DEFAULT_MINE_COUNT)
	{
		_window = window;

		_width = width;
		_height = height;

		_direction = Vector2.Zero;
		_selected = _area % 2 == 0 ? _area / 2 - _width / 2  - 1: _area / 2;

		_mineCount = Math.Min(mineCount, _area - 1);
		_gridCells = new MinesweeperCell[_area];

		for (int i = 0; i < _area; i++)
		{
			try 
			{
				MinesweeperCell cell = _gridCellTemplate.Instantiate() as MinesweeperCell;

				// Use the first cell to figure out board size information

				if (i == 0)
				{
					int cWidth = Mathf.CeilToInt(cell.GetRect().Size.X);
					int cHeight = Mathf.CeilToInt(cell.GetRect().Size.Y);

					_cellOffset = new Vector2(cWidth + _spacing, cHeight + _spacing);
					_gridLayout.AddThemeConstantOverride("h_separation", (int) _cellOffset.X);
					_gridLayout.AddThemeConstantOverride("v_separation", (int) _cellOffset.Y);
					

					float sizeX = (2 * _padding) + (cWidth * _width) + (_spacing * (_width - 1));
					float sizeY = (2 * _padding) + (cHeight * _height) + (_spacing * (_height - 1));

					_background.SetSize(new Vector2(sizeX, sizeY));
					_gridLayout.SetSize(new Vector2(sizeX - 2 * _padding, sizeY - 2 * _padding));
					_background.GetThemeStylebox("panel").Set("bg_color", ColorOperations.Mix(_bgTint, _window.PlayerColor, _bgBlendMode));
				}

				_gridLayout.AddChild(cell);

				_gridLayout.PivotOffset = new Vector2(_gridLayout.Size.X / 2, _gridLayout.Size.Y / 2);
				_background.PivotOffset = new Vector2(_background.Size.X / 2, _background.Size.Y / 2);
				_gridLayout.SetPosition(-_gridLayout.PivotOffset);
				_background.SetPosition(-_background.PivotOffset);

				cell.Init(this, i);
				_gridCells[i] = cell;
			}
			catch (InvalidCastException)
			{
				GD.PrintErr("Cell scene must have the MinesweeperCell script as its root type.");
			}
			catch (Exception e)
			{
				GD.PrintErr(e);
			}
		}

		if (_background.Size.X > _window.SqueezeSize.X || _background.Size.Y > _window.SqueezeSize.Y)
		{
			float ratioX = _window.SqueezeSize.X / _background.Size.X;
			float ratioY = _window.SqueezeSize.Y / _background.Size.Y;
			float newScale = Mathf.Min(ratioX, ratioY);

			Scale = new Vector2(newScale, newScale);

			if (_width % 2 == 0)
			{
				_zoomPos.X += _cellOffset.X / 2 * (_window.ZoomIn / newScale);
			}
			if (_height % 2 == 0)
			{
				_zoomPos.Y += _cellOffset.Y / 2 * (_window.ZoomIn / newScale);
			}

			if (newScale < _window.ZoomStartScale)
			{
				_shouldZoom = true;
			}
		}

		_gridLayout.Columns = _width;

		GD.Print(_gridAnchor.Position);
	}

	public void TryStart()
	{
		GD.Print(_gridAnchor.Position);
		if (_shouldZoom)
		{
			_canZoom = true;
			_isZooming = true;
		}
		else
		{
			Start();
		}
	}

	private void Start()
	{
		_active = true;
		_gridCells[_selected].Select();
	}

	private void End()
	{
		EmitSignal(SignalName.GridAnimationComplete);
	}

    public override void _Process(double delta)
    {
		if (_canZoom) ProcessZoom(delta);

		if (_active) ProcessInput(delta);
    }

	private void ProcessZoom(double delta)
	{
		if (_isZooming)
		{
			if (_shouldZoom)
			{
				if (_zoomProgress < 1)
				{
					_zoomProgress += delta / _zoomDuration;
				}
				else
				{
					_zoomProgress = 1;
					_isZooming = false;
					Start();
				}
			}
			else
			{
				if (_zoomProgress > 0)
				{
					_zoomProgress -= delta / _zoomDuration;
				}
				else
				{
					_zoomProgress = 0;
					_isZooming = false;
					End();
				}
			}

			float f = _zoomCurve.Sample((float) _zoomProgress);
			float anchorScale = _window.ZoomIn / Scale.X * f + (1 - f);

			_gridAnchor.Scale = new Vector2(anchorScale, anchorScale);
			_gridAnchor.Position = _zoomPos * f + Vector2.Zero * (1 - f);
		}
		else if (_shouldZoom && !_isZooming)
		{
			Vector2 displacement = _zoomPos - _gridAnchor.Position;

			float speed = Mathf.Clamp(Mathf.Pow(displacement.Length(), 2), _zoomPanMinSpeed, _zoomPanMaxSpeed) * ((float) delta);
			Vector2 movement = displacement.Normalized() * Mathf.Min(displacement.Length(), speed);
			
			_gridAnchor.Position = _gridAnchor.Position + movement;
		}
	}

	private void ProcessInput(double delta)
	{
        if (!_direction.Equals(_window.Input.Direction))
		{
			if (_direction.Equals(Vector2.Zero)) _moveProgress = 0.9;
			else _moveProgress = 0.5;

			_direction = _window.Input.Direction;
		}

		_moveProgress += (_window.Input.Fast ? _speed * _fastFactor : _speed) * delta;
		if (_moveProgress >= 1)
		{
			_gridCells[_selected].Deselect();

			_moveProgress = 0;
			int[] adjacent = GetAdjacentCells(_selected);

			if (_direction.Equals(Vector2.Left) && adjacent[(int) Direction.WEST] >= 0)
			{
				_selected = adjacent[(int) Direction.WEST];
				_zoomPos += _gridAnchor.Scale * _cellOffset * Vector2.Right;
			}
			else if (_direction.Equals(Vector2.Right) && adjacent[(int) Direction.EAST] >= 0)
			{
				_selected = adjacent[(int) Direction.EAST];
				_zoomPos += _gridAnchor.Scale * _cellOffset * Vector2.Left;
			}
			else if (_direction.Equals(Vector2.Up) && adjacent[(int) Direction.NORTH] >= 0)
			{
				_selected = adjacent[(int) Direction.NORTH];
				_zoomPos += _gridAnchor.Scale * _cellOffset * Vector2.Down;
			}
			else if (_direction.Equals(Vector2.Down) && adjacent[(int) Direction.SOUTH] >= 0)
			{
				_selected = adjacent[(int) Direction.SOUTH];
				_zoomPos += _gridAnchor.Scale * _cellOffset * Vector2.Up;
			}

			_gridCells[_selected].Select();
		}
	}

    private void Fill(int safeSpace)
	{
		HashSet<int> filled = new HashSet<int>
		{
			safeSpace
		};

		int planted = 0;
		while (planted < _mineCount)
		{
			int space = Mathf.FloorToInt(_rng.Randf() * _area) % _area;
			if (!filled.Contains(space))
			{
				filled.Add(space);
				_gridCells[space].AddMine();

				planted += 1;
			}
		}

		for (int i = 0; i < _area; i++)
		{
			int[] neighbors = GetAdjacentCells(i);
			int count = 0;

			for (int n = 0; n < 8; n++)
			{
				int neighbor = neighbors[n];

				if (neighbor >= 0 && _gridCells[neighbor].IsMine) count++;
			}

			_gridCells[i].AddHint(count);
		}
	}

	enum Direction { WEST, EAST, NORTH, SOUTH, NORTHWEST, SOUTHWEST, NORTHEAST, SOUTHEAST };

	private int[] GetAdjacentCells(int index)
	{
		int[] neighbors = new int[8];

		if (index % _width > 0) neighbors[0] = index - 1;
		else neighbors[0] = -1;
		
		if (index % _width < _width - 1) neighbors[1] = index + 1;
		else neighbors[1] = -1;

		if (index >= _width) neighbors[2] = index - _width;
		else neighbors[2] = -1;

		if (index < _area - _width) neighbors[3] = index + _width;
		else neighbors[3] = -1;

		if (index % _width > 0 && index >= _width) neighbors[4] = index - 1 - _width;
		else neighbors[4] = -1;
		
		if (index % _width > 0 && index < _area - _width) neighbors[5] = index - 1 + _width;
		else neighbors[5] = -1;

		if (index % _width < _width - 1 && index >= _width) neighbors[6] = index + 1 - _width;
		else neighbors[6] = -1;

		if (index % _width < _width - 1 && index < _area - _width) neighbors[7] = index + 1 + _width;
		else neighbors[7] = -1;
		
		return neighbors;
	}

	public void Reveal()
	{
		if (!_active) return;

		if (_gridCells[_selected].IsFlagged) return;

		if (_gridCells[_selected].IsMine)
		{
			RevealAll(false);

			_active = false;
			EmitSignal(SignalName.GridGameOver, false);

			if (_shouldZoom) 
			{
				_shouldZoom = false;
				_isZooming = true;
			}
			else End();
		}
		else
		{
			if (!_gridCells[_selected].IsDecided)
			{
				Fill(_selected);
			}

			Queue<int> queue = new Queue<int>();

			queue.Enqueue(_selected);

			while (queue.Count > 0)
			{
				int cell = queue.Dequeue();

				bool reveal = _gridCells[cell].Reveal();
				if (reveal)
				{
					_revealed += 1;

					if (_gridCells[cell].IsEmpty && !_gridCells[cell].IsFlagged)
					{
						int[] neighbors = GetAdjacentCells(cell);

						for (int n = 0; n < 8; n++)
						{
							int neighbor = neighbors[n];
							if (neighbor >= 0) queue.Enqueue(neighbor);
						}
					}
				}
			}

			if (_revealed + _mineCount == _area)
			{
				RevealAll(true);

				_active = false;
				EmitSignal(SignalName.GridGameOver, true);

				if (_shouldZoom) 
				{
					_shouldZoom = false;
					_isZooming = true;
				}
				else End();
			}
		}
	}

	private void RevealAll(bool flag)
	{
		for (int i = 0; i < _area; i++)
		{
			if (_gridCells[i].IsMine && !_gridCells[i].IsFlagged)
			{
				if (flag)
				{
					_gridCells[i].Flag();
				}
				else
				{
					UnflaggedMines++;
				}
			}
			_gridCells[i].Reveal();
		}

		_gridCells[_selected].Deselect();
	}

	public void Flag()
	{
		_gridCells[_selected].Flag();
	}
}
