using System.Reflection;

namespace Chess.Shared
{
    public class BitBoard
    {
        public ulong board;
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

        public int[] GetCoordinates()
        {
            int log = (int)Math.Log2(this.board);
            int[] coordinates = new int[2];
            coordinates[0] = log % 8;
            coordinates[1] = log / 8;
            return coordinates;
        }

        //public void RotateLeft(int shift)
        //{
        //    this.board = (this.board << shift) | (this.board >> -shift);
        //}
        //public void RotateRight(int shift)
        //{
        //    this.board = (this.board >> shift) | (this.board << -shift);
        //}

    }
}
