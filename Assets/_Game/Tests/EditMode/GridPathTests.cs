using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class GridPathTests
    {
        private GridPath _path;
        private List<GridCell> _result;

        [SetUp]
        public void SetUp()
        {
            _path = new GridPath();
            _result = new List<GridCell>();
        }

        // ---------- WalkGrid ----------

        [Test]
        public void WalkGrid_OutsideCell_NotWalkable()
        {
            var grid = new WalkGrid(-2, -3, 4, 5, true);

            Assert.IsTrue(grid.IsWalkable(new GridCell(-2, -3)));
            Assert.IsTrue(grid.IsWalkable(new GridCell(1, 1)));
            Assert.IsFalse(grid.IsWalkable(new GridCell(2, 1)));
            Assert.IsFalse(grid.IsWalkable(new GridCell(-3, 0)));
        }

        [Test]
        public void WalkGrid_SetOutsideCell_Throws()
        {
            var grid = new WalkGrid(0, 0, 3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetBlocked(new GridCell(3, 0)));
        }

        [Test]
        public void WalkGrid_SetRectPartlyOutside_ClipsToGrid()
        {
            var grid = new WalkGrid(0, 0, 3, 3);

            grid.SetRect(-1, -1, 3, 3, true);

            Assert.IsTrue(grid.IsWalkable(new GridCell(0, 0)));
            Assert.IsTrue(grid.IsWalkable(new GridCell(1, 1)));
            Assert.IsFalse(grid.IsWalkable(new GridCell(2, 2)));
        }

        // ---------- FindPath ----------

        [Test]
        public void FindPath_OpenGrid_StraightLineIncludingBothEnds()
        {
            var grid = new WalkGrid(0, 0, 10, 5, true);

            Assert.IsTrue(_path.FindPath(grid, new GridCell(1, 2), new GridCell(6, 2), _result));

            Assert.AreEqual(6, _result.Count);
            for (int i = 0; i < _result.Count; i++)
                Assert.AreEqual(new GridCell(1 + i, 2), _result[i]);
        }

        [Test]
        public void FindPath_SameCell_SingleCell()
        {
            var grid = new WalkGrid(0, 0, 3, 3, true);

            Assert.IsTrue(_path.FindPath(grid, new GridCell(1, 1), new GridCell(1, 1), _result));

            CollectionAssert.AreEqual(new[] { new GridCell(1, 1) }, _result);
        }

        [Test]
        public void FindPath_WallWithGap_GoesThroughGapWithShortestLength()
        {
            // Tường dọc x = 3 từ y = 0 tới y = 5, chừa khe ở y = 6.
            var grid = new WalkGrid(0, 0, 7, 7, true);
            grid.SetRect(3, 0, 1, 6, false);

            Assert.IsTrue(_path.FindPath(grid, new GridCell(0, 0), new GridCell(6, 0), _result));

            AssertValidPath(grid, _result, new GridCell(0, 0), new GridCell(6, 0));
            // Lên 6, sang 6, xuống 6 = 18 bước.
            Assert.AreEqual(19, _result.Count);
            CollectionAssert.Contains(_result, new GridCell(3, 6));
        }

        [Test]
        public void FindPath_DiagonalTarget_FewestTurns()
        {
            var grid = new WalkGrid(0, 0, 8, 8, true);

            Assert.IsTrue(_path.FindPath(grid, new GridCell(0, 0), new GridCell(5, 4), _result));

            AssertValidPath(grid, _result, new GridCell(0, 0), new GridCell(5, 4));
            Assert.AreEqual(10, _result.Count);
            Assert.AreEqual(1, CountTurns(_result));
        }

        [Test]
        public void FindPath_NoRoute_FalseAndEmpty()
        {
            var grid = new WalkGrid(0, 0, 5, 5, true);
            grid.SetRect(2, 0, 1, 5, false);

            Assert.IsFalse(_path.FindPath(grid, new GridCell(0, 0), new GridCell(4, 4), _result));
            Assert.AreEqual(0, _result.Count);
        }

        [Test]
        public void FindPath_TargetBlocked_False()
        {
            var grid = new WalkGrid(0, 0, 5, 5, true);
            grid.SetBlocked(new GridCell(3, 3));

            Assert.IsFalse(_path.FindPath(grid, new GridCell(0, 0), new GridCell(3, 3), _result));
        }

        [Test]
        public void FindPath_StartBlocked_StillLeaves()
        {
            var grid = new WalkGrid(0, 0, 5, 5, true);
            grid.SetBlocked(new GridCell(0, 0));

            Assert.IsTrue(_path.FindPath(grid, new GridCell(0, 0), new GridCell(2, 0), _result));
            Assert.AreEqual(3, _result.Count);
        }

        [Test]
        public void FindPath_NegativeCoordinates_Works()
        {
            var grid = new WalkGrid(-10, -5, 12, 8, true);
            grid.SetRect(-6, -5, 1, 7, false);

            Assert.IsTrue(_path.FindPath(grid, new GridCell(-9, -4), new GridCell(0, -4), _result));

            AssertValidPath(grid, _result, new GridCell(-9, -4), new GridCell(0, -4));
        }

        [Test]
        public void FindPath_ReusedForDifferentGrids_StaysCorrect()
        {
            var small = new WalkGrid(0, 0, 3, 3, true);
            var large = new WalkGrid(0, 0, 20, 20, true);
            large.SetRect(10, 0, 1, 19, false);

            Assert.IsTrue(_path.FindPath(small, new GridCell(0, 0), new GridCell(2, 2), _result));
            Assert.IsTrue(_path.FindPath(large, new GridCell(0, 0), new GridCell(19, 0), _result));
            AssertValidPath(large, _result, new GridCell(0, 0), new GridCell(19, 0));
            Assert.IsTrue(_path.FindPath(small, new GridCell(2, 2), new GridCell(0, 0), _result));
            Assert.AreEqual(5, _result.Count);
        }

        // ---------- FindNearestReachable ----------

        [Test]
        public void FindNearestReachable_TargetWalkable_ReturnsTarget()
        {
            var grid = new WalkGrid(0, 0, 5, 5, true);

            Assert.IsTrue(_path.FindNearestReachable(grid, new GridCell(0, 0), new GridCell(3, 2), out GridCell cell));
            Assert.AreEqual(new GridCell(3, 2), cell);
        }

        [Test]
        public void FindNearestReachable_TargetInsideBlock_ReturnsClosestEdge()
        {
            var grid = new WalkGrid(0, 0, 9, 9, true);
            grid.SetRect(3, 3, 3, 3, false);

            Assert.IsTrue(_path.FindNearestReachable(grid, new GridCell(0, 4), new GridCell(4, 4), out GridCell cell));

            // Mọi ô cạnh khối đều cách tâm 2; chọn ô đi bộ gần nhất từ (0, 4).
            Assert.AreEqual(new GridCell(2, 4), cell);
        }

        [Test]
        public void FindNearestReachable_IgnoresUnreachableIsland()
        {
            // Ô (6, 6) đi được nhưng bị rào kín; chạm vào đó thì đi tới chỗ gần nhất ngoài rào.
            var grid = new WalkGrid(0, 0, 10, 10, true);
            grid.SetRect(5, 5, 3, 3, false);
            grid.SetWalkable(new GridCell(6, 6), true);

            Assert.IsTrue(_path.FindNearestReachable(grid, new GridCell(0, 0), new GridCell(6, 6), out GridCell cell));

            Assert.AreEqual(2, GridCell.Manhattan(cell, new GridCell(6, 6)));
            Assert.IsTrue(_path.FindPath(grid, new GridCell(0, 0), cell, _result));
        }

        // ---------- FindStandCell (bố cục Khu 1 của Map D) ----------

        // Map D cao 26: luống [13, 10, 6, 2] → y 14..15; luống [13, 13, 6, 2] → y 11..12; giữa là y 13.
        private const int BedX = 13;
        private const int UpperBedTop = 15;
        private const int UpperBedBottom = 14;
        private const int LowerBedTop = 12;
        private const int LowerBedBottom = 11;
        private const int GapRow = 13;

        private static WalkGrid CreateZone1Grid()
        {
            var grid = new WalkGrid(0, 0, 44, 26, true);
            grid.SetRect(BedX, UpperBedBottom, 6, 2, false);
            grid.SetRect(BedX, LowerBedBottom, 6, 2, false);
            return grid;
        }

        [TestCase(UpperBedTop, UpperBedTop + 1)]
        [TestCase(UpperBedBottom, GapRow)]
        [TestCase(LowerBedTop, GapRow)]
        [TestCase(LowerBedBottom, LowerBedBottom - 1)]
        public void FindStandCell_BedRows_StandsDirectlyAboveOrBelow(int plotY, int expectedY)
        {
            WalkGrid grid = CreateZone1Grid();

            for (int x = BedX; x < BedX + 6; x++)
            {
                Assert.IsTrue(_path.FindStandCell(grid, new GridCell(x, plotY), new GridCell(2, 2), out GridCell stand));
                Assert.AreEqual(new GridCell(x, expectedY), stand);
            }
        }

        [Test]
        public void FindStandCell_CharacterBesideBed_StillPrefersAboveOrBelow()
        {
            WalkGrid grid = CreateZone1Grid();

            Assert.IsTrue(_path.FindStandCell(grid, new GridCell(BedX, UpperBedTop), new GridCell(BedX - 1, UpperBedTop), out GridCell stand));

            Assert.AreEqual(new GridCell(BedX, UpperBedTop + 1), stand);
        }

        [Test]
        public void FindStandCell_BothAboveAndBelowOpen_PicksCloserToCharacter()
        {
            var grid = new WalkGrid(0, 0, 10, 10, true);
            grid.SetBlocked(new GridCell(5, 5));

            Assert.IsTrue(_path.FindStandCell(grid, new GridCell(5, 5), new GridCell(5, 0), out GridCell stand));

            Assert.AreEqual(new GridCell(5, 4), stand);
        }

        [Test]
        public void FindStandCell_AboveAndBelowBlocked_UsesSide()
        {
            var grid = new WalkGrid(0, 0, 10, 10, true);
            grid.SetRect(5, 4, 1, 3, false);

            Assert.IsTrue(_path.FindStandCell(grid, new GridCell(5, 5), new GridCell(9, 5), out GridCell stand));

            Assert.AreEqual(new GridCell(6, 5), stand);
        }

        [Test]
        public void FindStandCell_AllNeighboursBlocked_UsesNearestReachable()
        {
            var grid = new WalkGrid(0, 0, 10, 10, true);
            grid.SetRect(4, 4, 3, 3, false);

            Assert.IsTrue(_path.FindStandCell(grid, new GridCell(5, 5), new GridCell(0, 5), out GridCell stand));

            Assert.AreEqual(2, GridCell.Manhattan(stand, new GridCell(5, 5)));
            Assert.IsTrue(grid.IsWalkable(stand));
        }

        [Test]
        public void FindStandCell_CharacterOutsideGrid_False()
        {
            var grid = new WalkGrid(0, 0, 5, 5, true);

            Assert.IsFalse(_path.FindStandCell(grid, new GridCell(2, 2), new GridCell(-1, 0), out _));
        }

        // ---------- Helpers ----------

        private static void AssertValidPath(WalkGrid grid, List<GridCell> path, GridCell from, GridCell to)
        {
            Assert.IsTrue(path.Count > 0);
            Assert.AreEqual(from, path[0]);
            Assert.AreEqual(to, path[path.Count - 1]);
            for (int i = 1; i < path.Count; i++)
            {
                Assert.AreEqual(1, GridCell.Manhattan(path[i - 1], path[i]), $"Bước {i} không liền kề.");
                Assert.IsTrue(grid.IsWalkable(path[i]), $"Ô {path[i]} bị chặn.");
            }
        }

        private static int CountTurns(List<GridCell> path)
        {
            int turns = 0;
            for (int i = 2; i < path.Count; i++)
            {
                int dx1 = path[i - 1].X - path[i - 2].X;
                int dy1 = path[i - 1].Y - path[i - 2].Y;
                int dx2 = path[i].X - path[i - 1].X;
                int dy2 = path[i].Y - path[i - 1].Y;
                if (dx1 != dx2 || dy1 != dy2)
                    turns++;
            }
            return turns;
        }
    }
}
