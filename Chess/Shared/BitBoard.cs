using System.Reflection;

namespace Chess.Shared
{
    public class BitBoard
    {
        public ulong board;
        private static string[] FileLetters = { "a", "b", "c", "d", "e", "f", "g", "h" };
        private const ulong notAFile = 0xfefefefefefefefe; // ~0x0101010101010101
        private const ulong notHFile = 0x7f7f7f7f7f7f7f7f; // ~0x8080808080808080
        public BitBoard(ulong board)
        {
            this.board = board;
        }
        public BitBoard(BitBoard board)
        {
            this.board = board.board;
        }
        public BitBoard(String notation)
        {
            int shift = 0;
            int fileLetterIndex = Array.IndexOf(FileLetters, notation[0].ToString().ToLower());
            if (fileLetterIndex < 0) throw new Exception("Invalid Square Notation: " + notation);
            shift += fileLetterIndex;
            int rowNumber = int.Parse(notation[1].ToString());
            if (rowNumber < 1 || rowNumber > 8) throw new Exception("Invalid Square Notation: " + notation);
            shift += (rowNumber - 1) * 8;
            this.board = (ulong)1 << shift;
        }

        public bool IsEqual(BitBoard other)
        {
            return board == other.board;
        }
        public bool IsEqual(ulong other)
        {
            return board == other;
        }

        public BitBoard UnionWith(BitBoard other)
        {
            return new BitBoard(this.board | other.board);
        }
        public BitBoard UnionWith(ulong other)
        {
            return new BitBoard(this.board | other);
        }
        public BitBoard IntersectionOf(BitBoard other)
        {
            return new BitBoard(this.board & other.board);
        }
        public BitBoard IntersectionOf(ulong other)
        {
            return new BitBoard(this.board & other);
        }

        public bool IntersectsWith(BitBoard other)
        {
            return (this.board & other.board) != 0;
        }
        public bool IntersectsWith(ulong other)
        {
            return (this.board & other) != 0;
        }

        public BitBoard GenShift(int shift)
        {
            this.board = (shift > 0) ? (this.board << shift) : (this.board >> -shift);
            return this;
        }

        public BitBoard StepOne(int shift)
        {
            if (shift > 9 || shift < -9)
            {
                throw new Exception("shift value out of range. Must be from -9 to 9 inclusive.");
            }
            if (shift == 1 || shift == 9 || shift == -7) this.board = this.board & notHFile;
            if (shift == -1 || shift == -9 || shift == 7) this.board = this.board & notAFile;
            return this.GenShift(shift);
        }

        public List<BitBoard> Enumerate()
        {
            return Enumerate(0, 64);
        }
        private List<BitBoard> Enumerate(int offset, int width)
        {
            List<BitBoard> moves = new();
            width /= 2;
            ulong mask = Convert.ToUInt64(Math.Pow(2, width)-1);
            mask <<= offset;
            if((board & mask) != 0)
            {
                if(width == 1)
                {
                    moves.Add(new BitBoard(mask));
                } else
                {
                    moves.AddRange(Enumerate(offset, width));
                }
            }
            mask <<= width;
            if ((board & mask) != 0)
            {
                if (width == 1)
                {
                    moves.Add(new BitBoard(mask));
                }
                else
                {
                    moves.AddRange(Enumerate(offset+width, width));
                }
            }
            return moves;
        }

        public int[] GetCoordinates()
        {
            int log = (int)Math.Log2(this.board);
            int[] coordinates = new int[2];
            coordinates[0] = log % 8;
            coordinates[1] = log / 8;
            return coordinates;
        }
        public string[] GetFileAndRank()
        {
            int[] coordinates = this.GetCoordinates();
            string[] notation = new string[2];
            notation[0] = BitBoard.FileLetters[coordinates[0]];
            notation[1] = (coordinates[1]+1).ToString();
            return notation;
        }

        public BitBoard RayAttack(int direction, BitBoard occupiedSquares)
        {
            ulong initialPosition = this.board;
            BitBoard flood = new(0);
            BitBoard propagator = new(this.board);
            if (direction > 9 || direction < -9)
            {
                throw new Exception("shift value out of range. Must be from -9 to 9 inclusive.");
            }
            for(int i = 0; i < 7; i++)
            {
                flood.board |= propagator.board &~occupiedSquares.board;
                propagator.StepOne(direction);
                propagator.board = propagator.board & ~occupiedSquares.board;
            }
            flood.board |= initialPosition;
            flood.StepOne(direction);
            return flood;
        }
        public IEnumerable<BitBoard> EnumeratedRayAttack(int direction, BitBoard occupiedSquares)
        {
            BitBoard propagator = new(this.board);
            if (direction > 9 || direction < -9)
            {
                throw new Exception("shift value out of range. Must be from -9 to 9 inclusive.");
            }
            for (int i = 0; i < 7; i++)
            {
                propagator.StepOne(direction);
                if (propagator.IsEqual(0)) break;
                yield return new(propagator);
                if ((propagator.board & occupiedSquares.board) != 0)
                {
                    break;
                };
            }
        }

        public BitBoard XRay(int direction, BitBoard occupiedSquares)
        {
            ulong initialPosition = this.board;
            ulong intersectOccupied = 0;
            BitBoard flood = new(0);
            BitBoard propagator = new(this.board);
            if (direction > 9 || direction < -9)
            {
                throw new Exception("shift value out of range. Must be from -9 to 9 inclusive.");
            }
            for (int i = 0; i < 7; i++)
            {
                flood.board |= propagator.board;
                propagator.StepOne(direction);
                if (intersectOccupied == 0)
                {
                    intersectOccupied = propagator.board & (occupiedSquares.board & ~initialPosition);
                }
                else if (propagator.IntersectsWith(occupiedSquares))
                {
                    flood.board |= propagator.board;
                    break;
                }
            }
            return flood;
        }

        public BitBoard AndNot(BitBoard board)
        {
            return new(this.board & ~board.board);
        }

        public string GetPositionGrid()
        {
            string output = "";
            string row = "";
            for (int i = 63; i >= 0; i--)
            {
                if (i != 63 && (i + 1) % 8 == 0)
                {
                    output += row + "\r\n";
                    row = "";
                }
                row = (((this.board >> i) & (uint)1) == 1 ? "x " : "- ") + row;
            }
            output += row + "\r\n";
            return output;
        }
        public void LogPosition()
        {
            Console.WriteLine(GetPositionGrid());
        }

    }
}
