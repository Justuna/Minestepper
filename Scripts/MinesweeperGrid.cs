using Godot;
using System;
using System.Collections.Generic;

public partial class MinesweeperGrid : Control
{
	[Export] private Control _background;
	[Export] private GridContainer _gridLayout;
	[Export] private PackedScene _gridCellTemplate;
	[Export] private float _spacing;
	[Export] private float _padding;

	private const int DEFAULT_WIDTH = 7;
	private const int DEFAULT_HEIGHT = 7;
	private const int DEFAULT_MINE_COUNT = 10;

	private MinesweeperCell[] _gridCells;
	private int _width;
	private int _height;
	private int _mineCount;
	private int _revealed = 0;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private int _area => _width * _height;

	public void Init(int width = DEFAULT_WIDTH, int height = DEFAULT_HEIGHT, int mineCount = DEFAULT_MINE_COUNT)
	{
		_width = width;
		_height = height;

		_gridLayout.Columns = _height;

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
					float sizeX = 2 * _padding + cell.GetRect().Size.X * _width + _spacing * (_width - 1);
					float sizeY = 2 * _padding + cell.GetRect().Size.Y * _height + _spacing * (_height - 1);

					_background.SetSize(new Vector2(sizeX, sizeY));
					_gridLayout.Set("h_separation", _spacing);
					_gridLayout.Set("v_separation", _spacing);
				}

				_gridLayout.AddChild(cell);
				
				//cell.Init(this, i);
				_gridCells[i] = cell;
				
			}
			catch (InvalidCastException)
			{
				GD.Print("ERROR: Cell scene must have the MinesweeperCell script as its root type.");
			}
			catch (Exception e)
			{
				GD.Print(e);
			}
		}
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
				if (neighbor > 0 && _gridCells[neighbor].IsMine) count++;
			}
			_gridCells[i].AddHint(count);
		}
	}

	private int[] GetAdjacentCells(int index)
	{
		int[] neighbors = new int[8];

		if (index % _width > 0) neighbors[0] = index - 1;
		else neighbors[0] = -1;
		
		if (index % _width < _width) neighbors[1] = index + 1;
		else neighbors[1] = -1;

		if (index >= _width) neighbors[2] = index - _width;
		else neighbors[2] = -1;

		if (index < _area - _width) neighbors[3] = index + _width;
		else neighbors[3] = -1;

		if (index % _width > 0 && index >= _width) neighbors[4] = index - 1 - _width;
		else neighbors[4] = -1;
		
		if (index % _width > 0 && index < _area - _width) neighbors[5] = index - 1 + _width;
		else neighbors[5] = -1;

		if (index % _width < _width && index >= _width) neighbors[6] = index + 1 - _width;
		else neighbors[6] = -1;

		if (index % _width < _width && index < _area - _width) neighbors[7] = index + 1 + _width;
		else neighbors[7] = -1;
		
		return neighbors;
	}

	public void Reveal(int index)
	{
		int area = _width * _height;

		if (_gridCells[index].IsMine)
		{
			for (int i = 0; i < area; i++)
			{
				_gridCells[i].Reveal();
			}

			// Do something on game loss
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
							if (neighbor > 0) queue.Enqueue(neighbor);
						}
					}
				}
			}

			if (_revealed + _mineCount == area)
			{
				// Do something on game win
			}
		}
	}
}
