namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using System;

[TestFixture]
public class Day17
{
    [Test]
    public void TestSamples()
    {
        var jet = ">>><<><>><<<>><>>><<<>>><<<><<<>><>><<>>";

        Puzzle(jet, 2022).Should().Be(3068L);
        Puzzle(jet, 1000000000000L).Should().Be(1514285714288L);
    }

    [Test]
    public void TestAocInput()
    {
        var jet = FileUtils.ReadAllText(this).Trim();

        Puzzle(jet, 2022).Should().Be(3137L);
        Puzzle(jet, 1000000000000L).Should().Be(1564705882327L);
    }

    // tetris shape where every row is encoded in binary for compact hittests with & operator
    // rows run from top (index 0) to bottom
    private record class Shape(int[] Rows, int Width);

    private static readonly Shape[] Shapes = new Shape[] {
        new Shape(new int[] { 8 + 4 + 2 + 1 }, 4),   // -
        new Shape(new int[] { 2, 4 + 2 + 1, 2 }, 3), // +
        new Shape(new int[] { 1, 1, 4 + 2 + 1 }, 3), // ┘
        new Shape(new int[] { 1, 1, 1, 1 }, 1),      // |
        new Shape(new int[] { 2 + 1, 2 + 1 }, 2),    // ■
    };

    // iterator for the jet, loops around
    private class JetIter
    {
        private readonly string _jet;
        private          long   _pos;

        public JetIter(string jet)
        {
            _jet = jet;
        }

        public long Pos => _pos % _jet.Length;

        public char Next() => _jet[(int)(_pos++ % _jet.Length)];
    }

    // containts (a window of) the endless tetris chamber
    // row index runs from bottom (index 0) to top, so the other way around than the shapes
    private class Chamber
    {
        // the chamber is endless and a lot of rocks are dropped but the class only keeps track of a small number of rows
        // why?
        // 1. theoretically more is necessary with specific constellations of shapes and jets but
        //    with these inputs here the highest _drop_ is around 30 units
        // 2. the _period_ length here is below 2000 rocks so an endless chamber is a bit overkill but
        //    it keeps the computations and the hashed state very compact and fast
        private const int Capacity = 40;

        private long _topRow    = 0L;
        private long _bottomRow = 0L;
        private readonly IDictionary<long, int> _rows = new Dictionary<long, int>(Capacity);

        public long Top    => _topRow;
        public long Bottom => _bottomRow;

        public long Height => _topRow - _bottomRow;

        // inserts new rows at the top and removes unnecessary rows at the bottom to keep the
        // chamber compact (remember only max drop rows are needed which is around 30 here)
        internal void InsertEmptyRowsAtTop(long numberOfEmptyRows)
        {
            for(long i = 0L; i < numberOfEmptyRows; i++)
            {
                _rows[++_topRow] = 0;
            }

            if(_topRow > _bottomRow + Capacity)
            {
                for(; _bottomRow < _topRow - Capacity; _bottomRow++)
                {
                    _rows.Remove(_bottomRow);
                }
            }
        }

        // tests if a new shape inserted at a chamber row overlaps already dropped shapes in the chamber
        internal bool HitTest(Shape shape, long row, int offset)
            => shape.Rows
                    .Where((line, idx) => ((line << offset) & _rows[row - idx]) > 0)
                    .Any();

        // inserts all lines of a shape at the final resting position starting from the top of the specified chamber row
        internal void StampShape(Shape shape, long row, int offset)
        {
            for(int i = 0; i < shape.Rows.Length; i++)
            {
                _rows[row - i] |= shape.Rows[i] << offset;
            }
        }

        public const long RowsEncodedInState = 32L;

        // encode the state of the tetris chamber as a bitwise combination of the first 32 lines and the current shape and jet
        // is used to detect periodic loops
        public (long, long, long, long, long) EncodeState(long shapeIdx, long jetIdx)
            => (jetIdx << 3 | shapeIdx,
                _rows[_topRow]      | (_rows[_topRow -  1] << 7) | (_rows[_topRow -  2] << 14) | (_rows[_topRow -  3] << 21) | (_rows[_topRow -  4] << 28) | (_rows[_topRow -  5] << 35) | (_rows[_topRow -  6] << 42) | (_rows[_topRow -  7] << 49),
                _rows[_topRow -  8] | (_rows[_topRow -  9] << 7) | (_rows[_topRow - 10] << 14) | (_rows[_topRow - 11] << 21) | (_rows[_topRow - 12] << 28) | (_rows[_topRow - 13] << 35) | (_rows[_topRow - 14] << 42) | (_rows[_topRow - 15] << 49),
                _rows[_topRow - 16] | (_rows[_topRow - 17] << 7) | (_rows[_topRow - 18] << 14) | (_rows[_topRow - 19] << 21) | (_rows[_topRow - 20] << 28) | (_rows[_topRow - 21] << 35) | (_rows[_topRow - 22] << 42) | (_rows[_topRow - 23] << 49),
                _rows[_topRow - 24] | (_rows[_topRow - 25] << 7) | (_rows[_topRow - 26] << 14) | (_rows[_topRow - 27] << 21) | (_rows[_topRow - 28] << 28) | (_rows[_topRow - 29] << 35) | (_rows[_topRow - 30] << 42) | (_rows[_topRow - 31] << 49));
    }

    // Large, oddly-shaped rocks are falling into the chamber from above, presumably due to all the rumbling. If you can't work out where
    // the rocks will fall next, you might be crushed!
    // The rocks don't spin, but they do get pushed around by jets of hot gas coming out of the walls themselves. In jet patterns, < means a
    // push to the left, while > means a push to the right.
    // The tall, vertical chamber is exactly seven units wide. After a rock appears, it alternates between being pushed by a jet of hot gas
    // one unit (in the direction indicated by the next symbol in the jet pattern) and then falling one unit down. If any movement would cause
    // any part of the rock to move into the walls, floor, or a stopped rock, the movement instead does not occur. If a downward movement would
    // have caused a falling rock to move into the floor or an already-fallen rock, the falling rock stops where it is and the next rock
    // immediately begins falling.
    //
    // Puzzle == How many units tall will the tower of rocks be after the specified number of rocks have stopped falling?
    private static long Puzzle(string jet, long nmbrRocks)
    {
        var chamber = new Chamber();
        var jetIter = new JetIter(jet);

        var topOfTower = 0L;

        // to determine the length of period map (shape, jet positions and chamber contents) to (current time and chamber top row)
        var states = new Dictionary<(long, long, long, long, long), (long, long)>(5000);

        for(long rock = 0L; rock < nmbrRocks; rock++)
        {
            // drop next shape 3 empty rows above the top of the tower before dropping the next shape

            var nextShape          = Shapes[rock % Shapes.Length];
            var dropNextShapeAtRow = topOfTower + 3L + nextShape.Rows.Length;

            chamber.InsertEmptyRowsAtTop(dropNextShapeAtRow - chamber.Top);

            var topRowOfDroppedShape = Drop(chamber, nextShape, dropNextShapeAtRow, jetIter);

            topOfTower = Math.Max(topOfTower, topRowOfDroppedShape);

            // check for period and if found directly compute result

            if(chamber.Height > Chamber.RowsEncodedInState)
            {
                var currentState = chamber.EncodeState(rock % Shapes.Length, jetIter.Pos);
                if(states.TryGetValue(currentState, out var periodStart))
                {
                    var periodLength = rock - periodStart.Item1;
                    var periodHeight = topOfTower - periodStart.Item2;
                    var nmbrPeriods  = (nmbrRocks - periodStart.Item1) / periodLength;

                    return periodStart.Item2 +
                           nmbrPeriods * periodHeight +
                           states.First(s => s.Value.Item1 - periodStart.Item1 ==
                                             (nmbrRocks - nmbrPeriods * periodLength - periodStart.Item1 - 1)).Value.Item2 - periodStart.Item2;
                }
                else
                {
                    states[currentState] = (rock, topOfTower);
                }
            }
        }

        // no period found in case when nmbrRocks < period length
        return topOfTower;
    }

    // drops a tetris shape and returns the top row of the shape where it came to rest in the chamber
    private static long Drop(Chamber chamber, Shape shape, long row, JetIter jetIter)
    {
        var offset = 7 - 2 - shape.Width;

        // no hittests necessary for the first 3 rows because they are empty
        // only test here if the shift tries to move a shape outside the chamber walls
        // this saves significant time because the average drop height is less than 3 (empty) + 2 (real)
        offset += jetIter.Next() == '<' ? 1 : -1; // first shift never hits a wall
        offset = ShiftWithoutHittestOtherTiles(shape, offset, jetIter.Next());
        offset = ShiftWithoutHittestOtherTiles(shape, offset, jetIter.Next());
        row -= 3;

        do
        {
            offset = Shift(chamber, shape, row, offset, jetIter.Next());
            if((row <= chamber.Bottom + shape.Rows.Length) || chamber.HitTest(shape, row - 1, offset))
            {
                break;
            }
        }
        while (row-- > chamber.Bottom + shape.Rows.Length);

        chamber.StampShape(shape, row, offset);

        return row;
    }

    private static int ShiftWithoutHittestOtherTiles(Shape shape, int offset, char shift)
        => shift switch {
            '<' => offset + shape.Width < 7 ? offset + 1 : offset,
            _   => offset > 0 ? offset - 1 : offset,
        };

    private static int Shift(Chamber chamber, Shape shape, long row, int offset, char shift)
        => shift switch {
            '<' => (offset + shape.Width < 7 && !chamber.HitTest(shape, row, offset + 1)) ? offset + 1 : offset,
            _   => (offset > 0 && !chamber.HitTest(shape, row, offset - 1)) ? offset - 1 : offset,
        };
}
