namespace Chess.Shared
{
    public class BitBoard
    {
        public ulong board;
        public BitBoard(ulong board)
        {
            this.board = board;
        }

        public bool isEqual(BitBoard other)
        {
            return board == other.board;
        }

        public void unionWith(BitBoard other)
        {
            this.board ^= other.board;
        }

    }

    public class Piece
    {
        public enum Type
        {
            Pawn = 0,
            Knight = 1,
            Bishop = 2,
            Rook = 3,
            Queen = 4,
            King = 5
        }

        public enum Colour
        {
            White = 0,
            Black = 1
        }

        public Colour colour;
        public Type type;
        public BitBoard position;

        public Piece(Colour colour, Type type, ulong position)
        {
            this.colour = colour;
            this.type = type;
            this.position = new BitBoard(position);
        }
    }

    public class Board
    {
        public Piece[] pieces;
        public Board(Piece[]? pieces)
        {
            if (pieces == null)
            {
                this.pieces = new Piece[0];
            } else
            {
                this.pieces = pieces;
            }
        }

        public void resetBoard()
        {
            this.pieces = new Piece[32];
            this.pieces[0] = new Piece(Piece.Colour.White, Piece.Type.Rook, 1);
            this.pieces[1] = new Piece(Piece.Colour.White, Piece.Type.Knight, 2);
            this.pieces[2] = new Piece(Piece.Colour.White, Piece.Type.Bishop, 4);
            this.pieces[3] = new Piece(Piece.Colour.White, Piece.Type.King, 8);
            this.pieces[4] = new Piece(Piece.Colour.White, Piece.Type.Queen, 0x10);
            this.pieces[5] = new Piece(Piece.Colour.White, Piece.Type.Bishop, 0x20);
            this.pieces[6] = new Piece(Piece.Colour.White, Piece.Type.Knight, 0x40);
            this.pieces[7] = new Piece(Piece.Colour.White, Piece.Type.Rook, 0x80);
            this.pieces[8] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x100);
            this.pieces[9] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x200);
            this.pieces[10] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x400);
            this.pieces[11] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x800);
            this.pieces[12] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x1000);
            this.pieces[13] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x2000);
            this.pieces[14] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x4000);
            this.pieces[15] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x8000);
            this.pieces[16] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x1000000000000);
            this.pieces[17] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x2000000000000);
            this.pieces[18] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x4000000000000);
            this.pieces[19] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x8000000000000);
            this.pieces[20] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x10000000000000);
            this.pieces[21] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x20000000000000);
            this.pieces[22] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x40000000000000);
            this.pieces[23] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x80000000000000);
            this.pieces[24] = new Piece(Piece.Colour.Black, Piece.Type.Rook, 0x100000000000000);
            this.pieces[25] = new Piece(Piece.Colour.Black, Piece.Type.Knight, 0x200000000000000);
            this.pieces[26] = new Piece(Piece.Colour.Black, Piece.Type.Bishop, 0x400000000000000);
            this.pieces[27] = new Piece(Piece.Colour.Black, Piece.Type.King, 0x800000000000000);
            this.pieces[28] = new Piece(Piece.Colour.Black, Piece.Type.Queen, 0x1000000000000000);
            this.pieces[29] = new Piece(Piece.Colour.Black, Piece.Type.Bishop, 0x2000000000000000);
            this.pieces[30] = new Piece(Piece.Colour.Black, Piece.Type.Knight, 0x4000000000000000);
            this.pieces[31] = new Piece(Piece.Colour.Black, Piece.Type.Rook, 0x8000000000000000);
        }

        public Piece? getPieceAtSquare(BitBoard squareSingleBitset)
        {
            for (int i = 0; i < this.pieces.Length; i++){
                if (this.pieces[i] != null && this.pieces[i].position.isEqual(squareSingleBitset)){
                    return this.pieces[i];
                }
            }
            return null;
        }

        public BitBoard getOccupiedSquares()
        {
            BitBoard occupiedSquares = new BitBoard(0);
            for(int i = 0; i < this.pieces.Length; i++)
            {
                occupiedSquares.unionWith(this.pieces[i].position);
            }
            return occupiedSquares;
        }
    }
}
