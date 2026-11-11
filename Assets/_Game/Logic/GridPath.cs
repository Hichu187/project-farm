using System;
using System.Collections.Generic;

namespace VeQue.Logic
{
    // Tìm đường cho nhân vật trên WalkGrid: đi 4 hướng, không cắt góc.
    // A* trên trạng thái (ô, hướng): mỗi bước tốn StepCost, mỗi lần rẽ thêm TurnCost (rất nhỏ) nên vẫn ra
    // đường ngắn nhất, và trong các đường ngắn nhất thì chọn đường ít rẽ nhất.
    // Bộ đệm được tái dùng giữa các lần gọi (không sinh rác mỗi lần chạm). Một instance không dùng song song.
    public sealed class GridPath
    {
        private const int StepCost = 1000;
        private const int TurnCost = 1;
        private const int DirCount = 4;

        // Lên, phải, xuống, trái (trục Y hướng lên).
        private static readonly int[] DirX = { 0, 1, 0, -1 };
        private static readonly int[] DirY = { 1, 0, -1, 0 };

        // A*: chỉ số trạng thái = ô * 4 + hướng vừa đi.
        private int[] _g = Array.Empty<int>();
        private int[] _parent = Array.Empty<int>();
        private int[] _openStamp = Array.Empty<int>();
        private int[] _closedStamp = Array.Empty<int>();
        private int _search;

        private int[] _heapState = new int[64];
        private int[] _heapF = new int[64];
        private int _heapCount;

        // Loang theo chiều rộng: khoảng cách đi bộ từ một ô tới mọi ô tới được.
        private int[] _dist = Array.Empty<int>();
        private int[] _distStamp = Array.Empty<int>();
        private int[] _floodOrder = Array.Empty<int>();
        private int _flood;

        private readonly List<int> _reverse = new List<int>();

        // Đường từ from tới to, gồm cả 2 đầu. from không cần đi được (nhân vật có thể đứng sai chỗ);
        // to phải đi được. Không có đường thì trả false và result rỗng.
        public bool FindPath(WalkGrid grid, GridCell from, GridCell to, List<GridCell> result)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            result.Clear();
            if (!grid.Contains(from) || !grid.IsWalkable(to))
                return false;
            if (from == to)
            {
                result.Add(from);
                return true;
            }

            EnsurePathBuffers(grid.CellCount * DirCount);
            _search++;
            _heapCount = 0;

            int startCell = grid.IndexOf(from);
            int goalCell = grid.IndexOf(to);
            int startF = GridCell.Manhattan(from, to) * StepCost;
            for (int dir = 0; dir < DirCount; dir++)
            {
                int state = startCell * DirCount + dir;
                _g[state] = 0;
                _parent[state] = -1;
                _openStamp[state] = _search;
                Push(state, startF);
            }

            while (TryPop(out int state))
            {
                if (_closedStamp[state] == _search)
                    continue;
                _closedStamp[state] = _search;

                int cellIndex = state / DirCount;
                if (cellIndex == goalCell)
                {
                    BuildPath(grid, state, result);
                    return true;
                }

                int dir = state % DirCount;
                GridCell cell = grid.CellAt(cellIndex);
                for (int next = 0; next < DirCount; next++)
                {
                    var neighbor = new GridCell(cell.X + DirX[next], cell.Y + DirY[next]);
                    if (!grid.IsWalkable(neighbor))
                        continue;

                    int nextState = grid.IndexOf(neighbor) * DirCount + next;
                    if (_closedStamp[nextState] == _search)
                        continue;

                    int g = _g[state] + StepCost + (next == dir ? 0 : TurnCost);
                    if (_openStamp[nextState] == _search && _g[nextState] <= g)
                        continue;

                    _openStamp[nextState] = _search;
                    _g[nextState] = g;
                    _parent[nextState] = state;
                    Push(nextState, g + GridCell.Manhattan(neighbor, to) * StepCost);
                }
            }
            return false;
        }

        // Ô đi được, tới được từ from, gần target nhất (khoảng cách Manhattan); bằng nhau thì chọn ô đi bộ ít hơn.
        // Dùng khi chạm vào chỗ không đi được (hàng rào, nhà, luống…).
        public bool FindNearestReachable(WalkGrid grid, GridCell from, GridCell target, out GridCell result)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            int count = Flood(grid, from);
            return TryPickNearest(grid, count, target, out result);
        }

        // Chỗ đứng để làm việc ở ô đất plot: ưu tiên ô ngay trên / dưới, rồi trái / phải, cuối cùng là ô
        // tới được gần nhất. Trong cùng nhóm chọn ô đi bộ ít hơn từ from.
        public bool FindStandCell(WalkGrid grid, GridCell plot, GridCell from, out GridCell result)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            int count = Flood(grid, from);
            if (count == 0)
            {
                result = default;
                return false;
            }

            if (TryPickCloser(grid, new GridCell(plot.X, plot.Y + 1), new GridCell(plot.X, plot.Y - 1), out result))
                return true;
            if (TryPickCloser(grid, new GridCell(plot.X - 1, plot.Y), new GridCell(plot.X + 1, plot.Y), out result))
                return true;
            return TryPickNearest(grid, count, plot, out result);
        }

        // Loang từ from qua các ô đi được. Trả số ô đã tới (thứ tự trong _floodOrder, khoảng cách trong _dist).
        private int Flood(WalkGrid grid, GridCell from)
        {
            if (!grid.Contains(from))
                return 0;

            EnsureFloodBuffers(grid.CellCount);
            _flood++;

            int start = grid.IndexOf(from);
            _dist[start] = 0;
            _distStamp[start] = _flood;
            _floodOrder[0] = start;
            int count = 1;

            for (int head = 0; head < count; head++)
            {
                int index = _floodOrder[head];
                GridCell cell = grid.CellAt(index);
                for (int dir = 0; dir < DirCount; dir++)
                {
                    var neighbor = new GridCell(cell.X + DirX[dir], cell.Y + DirY[dir]);
                    if (!grid.IsWalkable(neighbor))
                        continue;
                    int next = grid.IndexOf(neighbor);
                    if (_distStamp[next] == _flood)
                        continue;
                    _distStamp[next] = _flood;
                    _dist[next] = _dist[index] + 1;
                    _floodOrder[count++] = next;
                }
            }
            return count;
        }

        // Trong 2 ô a, b: ô đi được và tới được (theo lần Flood vừa chạy) có khoảng cách đi bộ nhỏ hơn; bằng nhau lấy a.
        private bool TryPickCloser(WalkGrid grid, GridCell a, GridCell b, out GridCell result)
        {
            int distA = ReachableDistance(grid, a);
            int distB = ReachableDistance(grid, b);
            if (distA < 0 && distB < 0)
            {
                result = default;
                return false;
            }
            result = distB < 0 || (distA >= 0 && distA <= distB) ? a : b;
            return true;
        }

        private int ReachableDistance(WalkGrid grid, GridCell cell)
        {
            if (!grid.IsWalkable(cell))
                return -1;
            int index = grid.IndexOf(cell);
            return _distStamp[index] == _flood ? _dist[index] : -1;
        }

        private bool TryPickNearest(WalkGrid grid, int count, GridCell target, out GridCell result)
        {
            result = default;
            bool found = false;
            int bestManhattan = int.MaxValue;
            int bestDist = int.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int index = _floodOrder[i];
                if (!grid.IsWalkableIndex(index))
                    continue;

                GridCell cell = grid.CellAt(index);
                int manhattan = GridCell.Manhattan(cell, target);
                int dist = _dist[index];
                if (manhattan < bestManhattan || (manhattan == bestManhattan && dist < bestDist))
                {
                    bestManhattan = manhattan;
                    bestDist = dist;
                    result = cell;
                    found = true;
                }
            }
            return found;
        }

        private void BuildPath(WalkGrid grid, int goalState, List<GridCell> result)
        {
            _reverse.Clear();
            for (int state = goalState; state >= 0; state = _parent[state])
                _reverse.Add(state / DirCount);
            for (int i = _reverse.Count - 1; i >= 0; i--)
                result.Add(grid.CellAt(_reverse[i]));
        }

        private void EnsurePathBuffers(int size)
        {
            if (_g.Length >= size)
                return;
            _g = new int[size];
            _parent = new int[size];
            _openStamp = new int[size];
            _closedStamp = new int[size];
            _search = 0;
        }

        private void EnsureFloodBuffers(int size)
        {
            if (_dist.Length >= size)
                return;
            _dist = new int[size];
            _distStamp = new int[size];
            _floodOrder = new int[size];
            _flood = 0;
        }

        // Đống nhị phân nhỏ nhất theo f; trạng thái cũ (đã có g tốt hơn) bị bỏ qua khi lấy ra.
        private void Push(int state, int f)
        {
            if (_heapCount == _heapState.Length)
            {
                Array.Resize(ref _heapState, _heapCount * 2);
                Array.Resize(ref _heapF, _heapCount * 2);
            }

            int i = _heapCount++;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_heapF[parent] <= f)
                    break;
                _heapState[i] = _heapState[parent];
                _heapF[i] = _heapF[parent];
                i = parent;
            }
            _heapState[i] = state;
            _heapF[i] = f;
        }

        private bool TryPop(out int state)
        {
            if (_heapCount == 0)
            {
                state = -1;
                return false;
            }

            state = _heapState[0];
            _heapCount--;
            if (_heapCount == 0)
                return true;

            int lastState = _heapState[_heapCount];
            int lastF = _heapF[_heapCount];
            int i = 0;
            while (true)
            {
                int child = i * 2 + 1;
                if (child >= _heapCount)
                    break;
                if (child + 1 < _heapCount && _heapF[child + 1] < _heapF[child])
                    child++;
                if (_heapF[child] >= lastF)
                    break;
                _heapState[i] = _heapState[child];
                _heapF[i] = _heapF[child];
                i = child;
            }
            _heapState[i] = lastState;
            _heapF[i] = lastF;
            return true;
        }
    }
}
