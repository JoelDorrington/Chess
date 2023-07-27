using System.Reflection;

namespace Chess.Shared
{
    public class BitBoard
    {
        public ulong board;
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
        
        public void GenShift(int shift)
        {
            this.board = (shift > 0) ? (this.board << shift) : (this.board >> -shift);
        }

        public void StepOne(int shift)
        {
            if (shift > 9 || shift < -9)
            {
                throw new Exception("shift value out of range. Must be from -9 to 9 inclusive.");
            }
            if (shift == 1 || shift == 9 || shift == -7) this.board = this.board & notHFile;
            if (shift == -1 || shift == -9 || shift == 7) this.board = this.board & notAFile;
            this.GenShift(shift);
        }

        public List<BitBoard> Enumerate()
        {
            List<BitBoard> result = new List<BitBoard>();
            for(int i = 0; i<64; i++)
            {
                if(!this.IntersectionOf((ulong)Math.Pow(2, i)).IsEqual(0))
                {
                    result.Add(new BitBoard((ulong)Math.Pow(2, i)));
                }
            }
            return result;
        }

        public int[] GetCoordinates()
        {
            int log = (int)Math.Log2(this.board);
            int[] coordinates = new int[2];
            coordinates[0] = log % 8;
            coordinates[1] = log / 8;
            return coordinates;
        }

        public BitBoard RayAttack(int direction, BitBoard occupiedSquares)
        {
            ulong initialPosition = this.board;
            Console.WriteLine($"Initial Position: {initialPosition:X}");
            BitBoard flood = new(0);
            BitBoard propagator = new(this.board);
            if (direction > 9 || direction < -9)
            {
                throw new Exception("shift value out of range. Must be from -9 to 9 inclusive.");
            }
            Console.WriteLine($"Direction: {direction}");
            for(int i = 0; i < 7; i++)
            {
                Console.WriteLine($"Propagator: {propagator.board:X}");
                flood.board |= propagator.board &~occupiedSquares.board;
                propagator.StepOne(direction);
                propagator.board = propagator.board & ~occupiedSquares.board;
            }
            flood.board |= initialPosition;
            flood.StepOne(direction);
            return flood;
        }

        public BitBoard AndNot(BitBoard board)
        {
            return new(this.board & ~board.board);
        }

    }
}
