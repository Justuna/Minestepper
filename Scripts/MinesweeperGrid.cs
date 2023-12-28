using Godot;
using System;
using System.Collections.Generic;

public partial class MinesweeperGrid : Control
{
	[Export] private Panel _background;
	[Export] private GridContainer _gridLayout;
	[Export] private PackedScene _gridCellTemplate;
	[Export] private int _spacing;
	[Export] private int _padding;
	[Export] private Color _bgTint;
	[Export] private BlendModes _bgBlendMode;

	private const int DEFAULT_WIDTH = 6;
	private const int DEFAULT_HEIGHT = 6;
	private const int DEFAULT_MINE_COUNT = 4;

	private MinesweeperCell[] _gridCells;
	private int _width;
	private int _height;
	private int _mineCount;
	private int _revealed = 0;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private int _area => _width * _height;

    public void Init(Color color, int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT, int mineCount = DEFAULT_MINE_COUNT)
	{
		_width = width;
		_height = height;

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

					_gridLayout.AddThemeConstantOverride("h_separation", cWidth + _spacing);
					_gridLayout.AddThemeConstantOverride("v_separation", cHeight + _spacing);

					float sizeX = (2 * _padding) + (cWidth * _width) + (_spacing * (_width - 1));
					float sizeY = (2 * _padding) + (cHeight * _height) + (_spacing * (_height - 1));

					_background.SetSize(new Vector2(sizeX, sizeY));
					_gridLayout.SetSize(new Vector2(sizeX - 2 * _padding, sizeY - 2 * _padding));
					_background.GetThemeStylebox("panel").Set("bg_color", ColorOperations.Mix(_bgTint, color, _bgBlendMode));
				}

				_gridLayout.AddChild(cell);

				_gridLayout.PivotOffset = new Vector2(_gridLayout.Size.X / 2, _gridLayout.Size.Y / 2);
				_background.PivotOffset = new Vector2(_background.Size.X / 2, _background.Size.Y / 2);
				_gridLayout.SetPosition(-_gridLayout.PivotOffset);
				_background.SetPosition(-_background.PivotOffset);

				cell.Init(this, i, color);
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

		_gridLayout.Columns = _width;
	}

	public void Fill(int safeSpace)
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

	public void Reveal(int index)
	{
		if (_gridCells[index].IsMine)
		{
			RevealAll();

			GD.Print("You lose!");
		}
		else
		{
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
							if (neighbor >= 0) queue.Enqueue(neighbor);
						}
					}
				}
			}

			if (_revealed + _mineCount == _area)
			{
				RevealAll();

				GD.Print("You win!");
			}
		}
	}

	private void RevealAll()
	{
		for (int i = 0; i < _area; i++)
		{
			_gridCells[i].Reveal();
		}
	}
}
