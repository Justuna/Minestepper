using System;
using Godot;

public partial class GridLevelTrack : Resource
{
    [Export] private GridLevel[] _track;
    [Export] private int _startLevel;

    public GridLevelTrack()
    {
        _track = null;
        _startLevel = 0;
    }

    public GridLevelTrack(GridLevel[] track, int startLevel)
    {
        _track = track;
        _startLevel = startLevel;
    }

    public int Length => _track.Length;

    public int StartLevel => Mathf.Min(_startLevel, _track.Length - 1);

    public GridLevel GetLevel(int level)
    {
        try
        {
            return _track[level];
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
}