namespace Downkyi.Core.Danmaku2Ass;

/// <summary>
/// Tracks row occupancy to prevent danmaku overlap.
/// For scrolling (type=1): each row stores the time when the previous danmaku will have
/// fully scrolled off the left edge so a new one can enter.
/// For fixed (type=4/5): each row stores the time when it becomes free again.
/// </summary>
internal class CollisionManager
{
    private readonly float[] _rowFreeAt;
    private readonly float _rowHeight;
    private readonly int _videoHeight;
    private readonly float _bottomMargin;

    internal CollisionManager(int videoHeight, float fontSize, float bottomMargin)
    {
        _videoHeight = videoHeight;
        _rowHeight = fontSize * 1.2f;
        _bottomMargin = bottomMargin;
        int maxRows = (int)((videoHeight * (1 - bottomMargin)) / _rowHeight);
        _rowFreeAt = new float[Math.Max(maxRows, 1)];
    }

    /// <summary>
    /// Finds the first available row for a danmaku entry and marks it occupied.
    /// Returns -1 if all rows are occupied (entry should be discarded or shown anyway).
    /// </summary>
    internal int AssignRow(DanmakuEntry entry, float clearTime)
    {
        for (int i = 0; i < _rowFreeAt.Length; i++)
        {
            if (_rowFreeAt[i] <= entry.Time)
            {
                _rowFreeAt[i] = clearTime;
                entry.Row = i;
                return i;
            }
        }
        return -1; // all rows occupied — show on last row anyway
    }

    internal float RowY(int row) => row * _rowHeight + _rowHeight;
}
