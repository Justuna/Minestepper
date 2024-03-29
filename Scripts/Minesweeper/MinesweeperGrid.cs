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
	[Export] private int _padding;
	[Export] private float _bgValueAdjustment;

	[ExportSubgroup("Animation Parameters")]
	[Export] private float _recenterDuration;
	[Export] private Curve _recenterCurve;
	[Export] private float _panMinSpeed;
	[Export] private float _panMaxSpeed;
	[Export] private float _zoomSpeed;
	[Export] private float _maxZoom = 5;
	[Export] private float _minZoom = 0.2f;

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
	private int _flagCount;
	private int _revealed = 0;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private int _area => _width * _height;
	private bool _active = false;

	private Vector2 _direction;
	private int _selected;
	private double _moveProgress = 0;

	
	private float _zoomScale = 1;
	private Vector2 _cellOffset;
	private Vector2 _cameraPos = Vector2.Zero;
	private bool _recentering = false;
	private float _recenterProgress = 0;
	

	[Signal]
	public delegate void GridGameOverEventHandler(bool win);
	[Signal]
	public delegate void GridAnimationCompleteEventHandler();
	[Signal]
	public delegate void GridFlagNumberChangedEventHandler(int flags);

	public PlayerWindow Window => _window;
	public int TotalMines => _mineCount;
	public int UnflaggedMines { get; private set; }
	public MinesweeperCell CurrentCell => _gridCells[_selected];

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

					_cellOffset = new Vector2(cWidth, cHeight);
					// _gridLayout.AddThemeConstantOverride("h_separation", (int) _cellOffset.X);
					// _gridLayout.AddThemeConstantOverride("v_separation", (int) _cellOffset.Y);

					float sizeX = (2 * _padding) + (cWidth * _width);
					float sizeY = (2 * _padding) + (cHeight * _height);

					_background.SetSize(new Vector2(sizeX, sizeY));
					_gridLayout.SetSize(new Vector2(sizeX - 2 * _padding, sizeY - 2 * _padding));
					_background.Modulate = ColorOperations.AdjustValue(_window.PlayerColor, _bgValueAdjustment, true);

					_gridLayout.PivotOffset = new Vector2(_gridLayout.Size.X / 2, _gridLayout.Size.Y / 2);
					_background.PivotOffset = new Vector2(_background.Size.X / 2, _background.Size.Y / 2);
					_gridLayout.SetPosition(-_gridLayout.PivotOffset);
					_background.SetPosition(-_background.PivotOffset);
				}

				_gridLayout.AddChild(cell);

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

			GD.Print(_window.SqueezeSize.X);
			GD.Print(newScale);

			Scale = new Vector2(newScale, newScale);
		}

		_gridLayout.Columns = _width;
	}

	public void Start()
	{
		_active = true;
		_gridCells[_selected].Select();
	}

    public override void _Process(double delta)
    {
		if (_active) 
		{
			ProcessCamera(delta);
			ProcessNavigation(delta);
		}

		if (_recentering)
		{
			RecenterAnimation(delta);
		}
    }

	private void ProcessCamera(double delta)
	{
		// Zoom

		float newScale = Mathf.Clamp
		(
			_zoomScale * (1 + _zoomSpeed * (float) delta * Window.Input.ZoomDirection), 
			_minZoom,
			_maxZoom
		);

		_gridAnchor.Scale = new Vector2(newScale, newScale);

		float shrinkGrow = (newScale / _zoomScale) - 1;
		_gridAnchor.Position += shrinkGrow * _gridAnchor.Position;
		_cameraPos += shrinkGrow * _cameraPos;

		_zoomScale = newScale;

		// Pan

		if (Window.Input.ZoomDirection == 0)
		{
			Vector2 displacement = _cameraPos - _gridAnchor.Position;

			float speed = Mathf.Clamp(Mathf.Pow(displacement.Length(), 2), _panMinSpeed, _panMaxSpeed) * ((float) delta);
			Vector2 movement = displacement.Normalized() * Mathf.Min(displacement.Length(), speed);
			
			_gridAnchor.Position += movement;
		}
	}

	private void ProcessNavigation(double delta)
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
			CurrentCell.Deselect();

			_moveProgress = 0;
			int[] adjacent = GetAdjacentCells(_selected);

			if (_direction.Equals(Vector2.Left) && adjacent[(int) Direction.WEST] >= 0)
			{
				_selected = adjacent[(int) Direction.WEST];
			}
			else if (_direction.Equals(Vector2.Right) && adjacent[(int) Direction.EAST] >= 0)
			{
				_selected = adjacent[(int) Direction.EAST];
			}
			else if (_direction.Equals(Vector2.Up) && adjacent[(int) Direction.NORTH] >= 0)
			{
				_selected = adjacent[(int) Direction.NORTH];
			}
			else if (_direction.Equals(Vector2.Down) && adjacent[(int) Direction.SOUTH] >= 0)
			{
				_selected = adjacent[(int) Direction.SOUTH];
			}

			// Get center of current cell relative to center of the board
			float offsetX = (_selected % _width) - (((float) _width - 1) / 2 );
			float offsetY = Mathf.Floor(_selected / _width) - (((float) _height - 1) / 2);

			_cameraPos = new Vector2(offsetX, offsetY) * _zoomScale * _cellOffset * -1;

			CurrentCell.Select();
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

		if (CurrentCell.IsFlagged) return;

		if (CurrentCell.IsRevealed)
		{
			if (CurrentCell.IsHint)
			{
				RevealHint();
			}
		}
		else
		{
			RevealHidden(_selected);
		}
	}

	private void RevealHint()
	{
		int[] neighbors = GetAdjacentCells(_selected);
		int count = 0;

		List<int> hiddenNeighbors = new();

		for (int n = 0; n < 8; n++)
		{
			int neighbor = neighbors[n];

			if (neighbor >= 0)
			{
				if (_gridCells[neighbor].IsFlagged)
				{
					count++;
				}
				else
				{
					hiddenNeighbors.Add(neighbor);
				}
			}
		}

		if (count >= CurrentCell.HintValue)
		{
			foreach (int neighbor in hiddenNeighbors)
			{
				// With each neighbor revealed, there is a chance that other neighbors 
				// are also revealed or that you ended the game

				if (!_gridCells[neighbor].IsRevealed) RevealHidden(neighbor);
				if (!_active) break;
			}
		}
	}

	private void RevealHidden(int index)
	{
		if (_gridCells[index].IsMine)
		{
			GameEnd(false);
		}
		else
		{
			if (!_gridCells[index].IsDecided)
			{
				Fill(index);
			}

			Queue<int> queue = new Queue<int>();

			queue.Enqueue(index);

			while (queue.Count > 0)
			{
				int cell = queue.Dequeue();

				bool reveal = _gridCells[cell].Reveal();
				if (reveal)
				{
					_revealed += 1;

					if (_gridCells[cell].IsEmpty)
					{
						int[] neighbors = GetAdjacentCells(cell);

						for (int n = 0; n < 8; n++)
						{
							int neighbor = neighbors[n];
							if (neighbor >= 0 && !_gridCells[neighbor].IsFlagged) queue.Enqueue(neighbor);
						}
					}
				}
			}

			if (_revealed + _mineCount == _area)
			{
				GameEnd(true);
			}
		}
	}

	public void Flag()
	{
		if (!CurrentCell.Flag()) return;

		if (CurrentCell.IsFlagged) _flagCount++;
		else _flagCount--;

		EmitSignal(SignalName.GridFlagNumberChanged, _flagCount);
	}

	private void GameEnd(bool win)
	{
		RevealAll(win);

		_active = false;
		_recentering = true;

		EmitSignal(SignalName.GridGameOver, win);
	}

	private void RevealAll(bool autoFlag)
	{
		for (int i = 0; i < _area; i++)
		{
			if (_gridCells[i].IsMine && !_gridCells[i].IsFlagged)
			{
				if (autoFlag)
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

	private void RecenterAnimation(double delta)
	{
		_recenterProgress += (float) delta / _recenterDuration;
		float f = _recenterCurve.Sample(_recenterProgress);

		_gridAnchor.Scale = (f + (1 - f) * _zoomScale) * Vector2.One;
		_gridAnchor.Position = f * Vector2.Zero + (1 - f) * _cameraPos;

		if (_recenterProgress >= 1)
		{
			_recenterProgress = 1;
			_recentering = false;
			EmitSignal(SignalName.GridAnimationComplete);
		}
	}
}
